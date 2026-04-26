using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.Services
{
    public interface IEmailCheckingService
    {
        Task<EmailCheckResult> CheckEmailAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class EmailCheckingService : IEmailCheckingService
    {
        private static readonly string[] CommonDkimSelectors =
        {
            "default", "selector1", "selector2", "google", "mail", "k1", "dkim"
        };

        private readonly IDnsAnalysisClient _dnsAnalysisClient;
        private readonly ILogger<EmailCheckingService> _logger;

        public EmailCheckingService(IDnsAnalysisClient dnsAnalysisClient, ILogger<EmailCheckingService> logger)
        {
            _dnsAnalysisClient = dnsAnalysisClient;
            _logger = logger;
        }

        public async Task<EmailCheckResult> CheckEmailAsync(string domain, CancellationToken cancellationToken = default)
        {
            var normalizedDomain = NormalizeDomain(domain);
            _logger.LogInformation("Email check started: {Domain}", normalizedDomain);

            var mxResult = await _dnsAnalysisClient.QueryAsync(normalizedDomain, "MX", cancellationToken);
            if (!mxResult.Succeeded)
            {
                return CreateDnsUnavailableResult(normalizedDomain, "MX lookup could not be completed.");
            }

            if (!mxResult.Records.Any())
            {
                // Skip the email module entirely when the domain does not advertise mail handling.
                return CreateNoMailServiceResult(normalizedDomain);
            }

            // Query SPF, DMARC, and a conservative DKIM selector set in parallel to keep DNS latency manageable.
            var txtTask = _dnsAnalysisClient.QueryAsync(normalizedDomain, "TXT", cancellationToken);
            var dmarcTask = _dnsAnalysisClient.QueryAsync($"_dmarc.{normalizedDomain}", "TXT", cancellationToken);
            var dkimTasks = CommonDkimSelectors.ToDictionary(
                selector => selector,
                selector => _dnsAnalysisClient.QueryAsync($"{selector}._domainkey.{normalizedDomain}", "TXT", cancellationToken));

            await Task.WhenAll(dkimTasks.Values.Append(txtTask).Append(dmarcTask));

            var txtResult = await txtTask;
            var dmarcResult = await dmarcTask;
            if (!txtResult.Succeeded || !dmarcResult.Succeeded || dkimTasks.Values.Any(task => !task.Result.Succeeded))
            {
                return CreateDnsUnavailableResult(normalizedDomain, "One or more DNS lookups for SPF, DKIM, or DMARC could not be completed.");
            }

            var dkimSelectorsFound = dkimTasks
                .Where(pair => pair.Value.Result.Records.Any(record => record.Contains("v=DKIM1", StringComparison.OrdinalIgnoreCase)))
                .Select(pair => pair.Key)
                .ToList();

            var spfRecord = txtResult.Records.FirstOrDefault(record => record.StartsWith("v=spf1", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            var effectiveSpfLookup = await ResolveEffectiveSpfRecordAsync(spfRecord, cancellationToken);
            if (!effectiveSpfLookup.Succeeded)
            {
                return CreateDnsUnavailableResult(normalizedDomain, "The SPF redirect lookup could not be completed.");
            }

            var effectiveSpfRecord = effectiveSpfLookup.Record;
            var dmarcRecord = dmarcResult.Records.FirstOrDefault(record => record.StartsWith("v=DMARC1", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

            var result = new EmailCheckResult
            {
                Domain = normalizedDomain,
                HasMailService = true,
                ModuleApplicable = true,
                DnsSummary = new EmailDnsSummary
                {
                    MxRecords = mxResult.Records.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                    SpfRecord = effectiveSpfRecord,
                    DmarcRecord = dmarcRecord,
                    DkimSelectorsFound = dkimSelectorsFound
                }
            };

            result.Criteria.SpfVerification = EvaluateSpf(spfRecord, effectiveSpfRecord);
            result.Criteria.DkimActivated = EvaluateDkim(dkimSelectorsFound);
            result.Criteria.DmarcEnforcement = EvaluateDmarc(dmarcRecord);

            result.OverallScore =
                result.Criteria.SpfVerification.Score +
                result.Criteria.DkimActivated.Score +
                result.Criteria.DmarcEnforcement.Score;

            result.Status = result.OverallScore >= 15 ? "PASS" : result.OverallScore >= 8 ? "WARNING" : "FAIL";

            AddAlerts(result, spfRecord, effectiveSpfRecord, dmarcRecord, dkimSelectorsFound);

            _logger.LogInformation("Email check completed: Domain={Domain}, Score={Score}, Status={Status}",
                result.Domain, result.OverallScore, result.Status);

            return result;
        }

        private static EmailScoreDetail EvaluateSpf(string originalSpfRecord, string effectiveSpfRecord)
        {
            if (string.IsNullOrWhiteSpace(effectiveSpfRecord))
            {
                return new EmailScoreDetail
                {
                    Score = 0,
                    Confidence = "HIGH",
                    Details = "No SPF record was found."
                };
            }

            if (effectiveSpfRecord.Contains("-all", StringComparison.OrdinalIgnoreCase))
            {
                return new EmailScoreDetail
                {
                    Score = 7,
                    Confidence = string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal) ? "HIGH" : "MEDIUM",
                    Details = string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal)
                        ? $"SPF is configured with a strict fail policy: {effectiveSpfRecord}"
                        : $"SPF is configured via redirect delegation and resolves to a strict fail policy: {effectiveSpfRecord}"
                };
            }

            if (effectiveSpfRecord.Contains("~all", StringComparison.OrdinalIgnoreCase))
            {
                return new EmailScoreDetail
                {
                    Score = 5,
                    Confidence = string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal) ? "HIGH" : "MEDIUM",
                    Details = string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal)
                        ? $"SPF is configured with a soft fail policy: {effectiveSpfRecord}"
                        : $"SPF is configured via redirect delegation and resolves to a soft fail policy: {effectiveSpfRecord}"
                };
            }

            return new EmailScoreDetail
            {
                Score = 3,
                Confidence = string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal) ? "HIGH" : "MEDIUM",
                Details = $"SPF is present, but the policy may be weaker than recommended: {effectiveSpfRecord}"
            };
        }

        private static EmailScoreDetail EvaluateDkim(List<string> selectorsFound)
        {
            if (selectorsFound.Any())
            {
                return new EmailScoreDetail
                {
                    Score = 7,
                    Confidence = "MEDIUM",
                    Details = $"DKIM was detected using common selector(s): {string.Join(", ", selectorsFound)}."
                };
            }

            return new EmailScoreDetail
            {
                Score = 5,
                Confidence = "LOW",
                Details = "DKIM could not be verified with the common selectors that were tested. This does not necessarily mean DKIM is not configured."
            };
        }

        private static EmailScoreDetail EvaluateDmarc(string dmarcRecord)
        {
            if (string.IsNullOrWhiteSpace(dmarcRecord))
            {
                return new EmailScoreDetail
                {
                    Score = 0,
                    Confidence = "HIGH",
                    Details = "No DMARC record was found."
                };
            }

            var dmarcPolicy = GetTagValue(dmarcRecord, "p");
            var percentage = GetTagValue(dmarcRecord, "pct");
            var effectivePercentage = int.TryParse(percentage, out var pctValue) ? pctValue : 100;

            if (string.Equals(dmarcPolicy, "reject", StringComparison.OrdinalIgnoreCase))
            {
                return new EmailScoreDetail
                {
                    Score = effectivePercentage < 100 ? 5 : 6,
                    Confidence = "HIGH",
                    Details = effectivePercentage < 100
                        ? $"DMARC enforcement is strong, but only applies to {effectivePercentage}% of messages: {dmarcRecord}"
                        : $"DMARC enforcement is strong: {dmarcRecord}"
                };
            }

            if (string.Equals(dmarcPolicy, "quarantine", StringComparison.OrdinalIgnoreCase))
            {
                return new EmailScoreDetail
                {
                    Score = effectivePercentage < 100 ? 3 : 4,
                    Confidence = "HIGH",
                    Details = effectivePercentage < 100
                        ? $"DMARC enforcement is moderate and only applies to {effectivePercentage}% of messages: {dmarcRecord}"
                        : $"DMARC enforcement is moderate: {dmarcRecord}"
                };
            }

            return new EmailScoreDetail
            {
                Score = 3,
                Confidence = "HIGH",
                Details = $"DMARC is present, but enforcement is weak: {dmarcRecord}"
            };
        }

        private static void AddAlerts(
            EmailCheckResult result,
            string originalSpfRecord,
            string effectiveSpfRecord,
            string dmarcRecord,
            List<string> dkimSelectorsFound)
        {
            if (string.IsNullOrWhiteSpace(effectiveSpfRecord))
            {
                result.Alerts.Add(new EmailAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "SPF is missing. This increases the risk of email spoofing."
                });
            }
            else if (!effectiveSpfRecord.Contains("-all", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new EmailAlert
                {
                    Type = "INFO",
                    Message = !string.Equals(originalSpfRecord, effectiveSpfRecord, StringComparison.Ordinal)
                        ? "SPF is present and uses redirect delegation."
                        : "SPF is present, but does not use a strict -all policy."
                });
            }

            if (!dkimSelectorsFound.Any())
            {
                result.Alerts.Add(new EmailAlert
                {
                    Type = "INFO",
                    Message = "DKIM could not be verified with the common selectors that were tested, so it was scored conservatively."
                });
            }

            if (string.IsNullOrWhiteSpace(dmarcRecord))
            {
                result.Alerts.Add(new EmailAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "DMARC is missing. This reduces protection against spoofed email."
                });
            }
            else if (string.Equals(GetTagValue(dmarcRecord, "p"), "none", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new EmailAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "DMARC is present, but enforcement is set to p=none."
                });
            }
        }

        private static EmailCheckResult CreateNoMailServiceResult(string domain)
        {
            return new EmailCheckResult
            {
                Domain = domain,
                HasMailService = false,
                ModuleApplicable = false,
                Status = "NOT_APPLICABLE",
                Alerts = new List<EmailAlert>
                {
                    new EmailAlert
                    {
                        Type = "INFO",
                        Message = "No MX record was found. The email security module is not applicable for this domain."
                    }
                }
            };
        }

        private static EmailCheckResult CreateDnsUnavailableResult(string domain, string details)
        {
            return new EmailCheckResult
            {
                Domain = domain,
                HasMailService = false,
                ModuleApplicable = true,
                Status = "ERROR",
                Alerts = new List<EmailAlert>
                {
                    new EmailAlert
                    {
                        Type = "WARNING",
                        Message = $"Email security DNS lookups could not be completed reliably. {details}"
                    }
                }
            };
        }

        private static string NormalizeDomain(string domain)
        {
            var trimmed = domain.Trim();

            if (trimmed.Contains('@'))
            {
                var mailParts = trimmed.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                trimmed = mailParts.Length > 1 ? mailParts[^1] : trimmed;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.Host;
            }

            return trimmed.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');
        }

        private async Task<(bool Succeeded, string Record)> ResolveEffectiveSpfRecordAsync(string spfRecord, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(spfRecord))
            {
                return (true, string.Empty);
            }

            // Follow SPF redirect delegation so outsourced mail platforms are scored from the effective policy, not the wrapper.
            var redirectTarget = GetTagValue(spfRecord, "redirect");
            if (string.IsNullOrWhiteSpace(redirectTarget))
            {
                return (true, spfRecord);
            }

            var redirectTxtResult = await _dnsAnalysisClient.QueryAsync(redirectTarget, "TXT", cancellationToken);
            if (!redirectTxtResult.Succeeded)
            {
                return (false, string.Empty);
            }

            var redirectedSpf = redirectTxtResult.Records.FirstOrDefault(record => record.StartsWith("v=spf1", StringComparison.OrdinalIgnoreCase));

            return (true, string.IsNullOrWhiteSpace(redirectedSpf) ? spfRecord : redirectedSpf);
        }

        private static string GetTagValue(string record, string tagName)
        {
            var segments = record.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var segment in segments)
            {
                var separatorIndex = segment.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var currentTag = segment[..separatorIndex].Trim();
                var currentValue = segment[(separatorIndex + 1)..].Trim();

                if (string.Equals(currentTag, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    return currentValue;
                }
            }

            return string.Empty;
        }
    }
}
