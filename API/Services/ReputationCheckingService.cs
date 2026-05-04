using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.Services
{
    public interface IReputationCheckingService
    {
        Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class ReputationCheckingService : IReputationCheckingService
    {
        private readonly IVirusTotalClient _virusTotalClient;
        private readonly IDnsAnalysisClient _dnsAnalysisClient;
        private readonly ILogger<ReputationCheckingService> _logger;

        public ReputationCheckingService(
            IVirusTotalClient virusTotalClient,
            IDnsAnalysisClient dnsAnalysisClient,
            ILogger<ReputationCheckingService> logger)
        {
            _virusTotalClient = virusTotalClient;
            _dnsAnalysisClient = dnsAnalysisClient;
            _logger = logger;
        }

        public async Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default)
        {
            var normalizedDomain = NormalizeDomain(domain);
            _logger.LogInformation("Reputation check started: {Domain}", normalizedDomain);

            var dnsResolution = await GetDnsResolutionStateAsync(normalizedDomain, cancellationToken);
            if (dnsResolution == DnsResolutionState.NotResolvable)
            {
                return CreateUnavailableResult(
                    normalizedDomain,
                    "DNS did not return A or AAAA records for this domain. Reputation was not scored because a non-resolving domain is not proven safe by absent VirusTotal detections.",
                    "DNS_NOT_FOUND");
            }

            // Pull one normalized report and translate it into a bounded score instead of exposing provider-specific noise directly.
            var report = await _virusTotalClient.GetDomainReportAsync(normalizedDomain, cancellationToken);
            if (report == null)
            {
                return CreateErrorResult(normalizedDomain, "VirusTotal data could not be retrieved. Check API key, quota, or domain availability.");
            }

            if (string.Equals(report.ProviderStatus, "NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                return CreateUnavailableResult(
                    normalizedDomain,
                    "VirusTotal has no domain report for this target. This is not evidence that the domain is clean.",
                    "NOT_FOUND");
            }

            if (string.Equals(report.ProviderStatus, "RATE_LIMITED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(report.ProviderStatus, "UNAVAILABLE", StringComparison.OrdinalIgnoreCase))
            {
                return CreateUnavailableResult(normalizedDomain, report.ProviderMessage, report.ProviderStatus);
            }

            if (HasNoVirusTotalEvidence(report))
            {
                return CreateUnavailableResult(
                    normalizedDomain,
                    "VirusTotal returned a domain object, but it contains no analysis evidence. This is not evidence that the domain is clean.",
                    "NO_EVIDENCE");
            }

            var result = new ReputationCheckResult
            {
                Domain = normalizedDomain,
                ProviderStatus = report.ProviderStatus,
                Summary = new ReputationSummary
                {
                    MaliciousDetections = report.MaliciousDetections,
                    SuspiciousDetections = report.SuspiciousDetections,
                    HarmlessDetections = report.HarmlessDetections,
                    UndetectedDetections = report.UndetectedDetections,
                    Reputation = report.Reputation,
                    CommunityMaliciousVotes = report.CommunityMaliciousVotes,
                    CommunityHarmlessVotes = report.CommunityHarmlessVotes,
                    LastAnalysisDate = report.LastAnalysisDate?.ToString("u") ?? string.Empty,
                    Permalink = report.Permalink
                }
            };

            result.Criteria.BlacklistStatus = EvaluateBlacklistStatus(report);
            result.Criteria.MalwareAssociation = EvaluateMalwareAssociation(report);

            result.OverallScore =
                result.Criteria.BlacklistStatus.Score +
                result.Criteria.MalwareAssociation.Score;

            result.Status = result.OverallScore >= 15 ? "PASS" : result.OverallScore >= 8 ? "WARNING" : "FAIL";

            AddAlerts(result, report);

            _logger.LogInformation("Reputation check completed: Domain={Domain}, Score={Score}, Status={Status}",
                result.Domain, result.OverallScore, result.Status);

            return result;
        }

        private static ReputationScoreDetail EvaluateBlacklistStatus(VirusTotalDomainReport report)
        {
            if (report.MaliciousDetections > 0)
            {
                return new ReputationScoreDetail
                {
                    Score = 0,
                    Confidence = "HIGH",
                    Details = $"VirusTotal reports {report.MaliciousDetections} malicious detection(s) and {report.SuspiciousDetections} suspicious detection(s)."
                };
            }

            if (report.SuspiciousDetections == 0)
            {
                return new ReputationScoreDetail
                {
                    Score = 10,
                    Confidence = "HIGH",
                    Details = "No blacklist-style malicious or suspicious detections were reported by VirusTotal."
                };
            }

            return new ReputationScoreDetail
            {
                Score = report.SuspiciousDetections <= 2 ? 6 : 3,
                Confidence = "HIGH",
                Details = $"VirusTotal reports {report.SuspiciousDetections} suspicious detection(s) and no malicious detections."
            };
        }

        private static ReputationScoreDetail EvaluateMalwareAssociation(VirusTotalDomainReport report)
        {
            var communityVotesAreNegative =
                report.CommunityMaliciousVotes > 1 &&
                (report.Reputation <= 0 || report.CommunityMaliciousVotes > report.CommunityHarmlessVotes);

            if (report.MaliciousDetections > 0 || communityVotesAreNegative || report.Reputation < -10)
            {
                return new ReputationScoreDetail
                {
                    Score = 0,
                    Confidence = "MEDIUM",
                    Details = $"Potential malware association was indicated by reputation={report.Reputation}, malicious detections={report.MaliciousDetections}, and community malicious votes={report.CommunityMaliciousVotes}."
                };
            }

            if (report.Reputation > 0 && report.CommunityMaliciousVotes == 0 && report.SuspiciousDetections == 0)
            {
                return new ReputationScoreDetail
                {
                    Score = 10,
                    Confidence = "MEDIUM",
                    Details = $"Reputation signals are strongly positive: reputation={report.Reputation}, community malicious votes={report.CommunityMaliciousVotes}."
                };
            }

            if (report.Reputation > 0 && report.CommunityMaliciousVotes > 0 && report.SuspiciousDetections == 0)
            {
                return new ReputationScoreDetail
                {
                    Score = 8,
                    Confidence = "MEDIUM",
                    Details = $"No malware association was indicated by detections. Community malicious votes={report.CommunityMaliciousVotes} were outweighed by harmless votes={report.CommunityHarmlessVotes} and positive reputation={report.Reputation}."
                };
            }

            if (report.Reputation >= 0 && report.CommunityMaliciousVotes == 0)
            {
                return new ReputationScoreDetail
                {
                    Score = 10,
                    Confidence = "MEDIUM",
                    Details = $"No strong malware association was indicated, but the reputation signal is neutral: reputation={report.Reputation}, community malicious votes={report.CommunityMaliciousVotes}."
                };
            }

            return new ReputationScoreDetail
            {
                Score = 6,
                Confidence = "MEDIUM",
                Details = $"Reputation signals are mixed: reputation={report.Reputation}, community malicious votes={report.CommunityMaliciousVotes}."
            };
        }

        private static void AddAlerts(ReputationCheckResult result, VirusTotalDomainReport report)
        {
            if (report.MaliciousDetections > 0)
            {
                result.Alerts.Add(new ReputationAlert
                {
                    Type = "CRITICAL_ALARM",
                    Message = $"VirusTotal reports {report.MaliciousDetections} malicious detection(s) for this domain."
                });
            }

            if (report.SuspiciousDetections > 0)
            {
                result.Alerts.Add(new ReputationAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = $"VirusTotal reports {report.SuspiciousDetections} suspicious detection(s) for this domain."
                });
            }

            if (report.CommunityMaliciousVotes > 0)
            {
                result.Alerts.Add(new ReputationAlert
                {
                    Type = "INFO",
                    Message = $"VirusTotal community users submitted {report.CommunityMaliciousVotes} malicious vote(s)."
                });
            }

            if (report.Reputation == 0 && report.MaliciousDetections == 0 && report.SuspiciousDetections == 0)
            {
                result.Alerts.Add(new ReputationAlert
                {
                    Type = "INFO",
                    Message = "VirusTotal reputation is neutral. No malicious or suspicious detections were reported, but that is not a guarantee that the domain is safe."
                });
            }

            if (string.IsNullOrWhiteSpace(report.Permalink) == false)
            {
                result.Alerts.Add(new ReputationAlert
                {
                    Type = "INFO",
                    Message = $"VirusTotal report link: {report.Permalink}"
                });
            }
        }

        private static string NormalizeDomain(string domain)
        {
            return DomainInputSanitizer.NormalizeDomain(domain);
        }

        private static bool HasNoVirusTotalEvidence(VirusTotalDomainReport report)
        {
            return report.Reputation == 0 &&
                   report.MaliciousDetections == 0 &&
                   report.SuspiciousDetections == 0 &&
                   report.HarmlessDetections == 0 &&
                   report.UndetectedDetections == 0 &&
                   report.CommunityMaliciousVotes == 0 &&
                   report.CommunityHarmlessVotes == 0;
        }

        private async Task<DnsResolutionState> GetDnsResolutionStateAsync(string domain, CancellationToken cancellationToken)
        {
            var aTask = _dnsAnalysisClient.QueryAsync(domain, "A", cancellationToken);
            var aaaaTask = _dnsAnalysisClient.QueryAsync(domain, "AAAA", cancellationToken);

            await Task.WhenAll(aTask, aaaaTask);

            var aResult = await aTask;
            var aaaaResult = await aaaaTask;

            if (aResult.Records.Count > 0 || aaaaResult.Records.Count > 0)
            {
                return DnsResolutionState.Resolvable;
            }

            if (aResult.Succeeded && aaaaResult.Succeeded)
            {
                return DnsResolutionState.NotResolvable;
            }

            return DnsResolutionState.Unknown;
        }

        private enum DnsResolutionState
        {
            Resolvable,
            NotResolvable,
            Unknown
        }

        private static ReputationCheckResult CreateErrorResult(string domain, string message)
        {
            return new ReputationCheckResult
            {
                Domain = domain,
                Status = "ERROR",
                ProviderStatus = "ERROR",
                Alerts = new List<ReputationAlert>
                {
                    new ReputationAlert
                    {
                        Type = "CRITICAL_WARNING",
                        Message = message
                    }
                }
            };
        }

        private static ReputationCheckResult CreateUnavailableResult(string domain, string message, string providerStatus = "UNAVAILABLE")
        {
            return new ReputationCheckResult
            {
                Domain = domain,
                Status = "UNAVAILABLE",
                ProviderStatus = providerStatus,
                Alerts = new List<ReputationAlert>
                {
                    new ReputationAlert
                    {
                        Type = "INFO",
                        Message = string.IsNullOrWhiteSpace(message)
                            ? "VirusTotal reputation data is temporarily unavailable."
                            : message
                    }
                }
            };
        }
    }
}
