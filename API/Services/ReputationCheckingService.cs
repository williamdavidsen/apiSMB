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
        private readonly ILogger<ReputationCheckingService> _logger;

        public ReputationCheckingService(IVirusTotalClient virusTotalClient, ILogger<ReputationCheckingService> logger)
        {
            _virusTotalClient = virusTotalClient;
            _logger = logger;
        }

        public async Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default)
        {
            var normalizedDomain = NormalizeDomain(domain);
            _logger.LogInformation("Reputation check started: {Domain}", normalizedDomain);

            // Pull one normalized report and translate it into a bounded score instead of exposing provider-specific noise directly.
            var report = await _virusTotalClient.GetDomainReportAsync(normalizedDomain, cancellationToken);
            if (report == null)
            {
                return CreateErrorResult(normalizedDomain, "VirusTotal data could not be retrieved. Check API key, quota, or domain availability.");
            }

            if (string.Equals(report.ProviderStatus, "RATE_LIMITED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(report.ProviderStatus, "UNAVAILABLE", StringComparison.OrdinalIgnoreCase))
            {
                return CreateUnavailableResult(normalizedDomain, report.ProviderMessage);
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
            if (report.MaliciousDetections > 0 || report.CommunityMaliciousVotes > 1 || report.Reputation < -10)
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
                    Message = "VirusTotal reputation is neutral. The domain appears clean, but does not have a strongly positive reputation signal."
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

        private static ReputationCheckResult CreateUnavailableResult(string domain, string message)
        {
            return new ReputationCheckResult
            {
                Domain = domain,
                Status = "UNAVAILABLE",
                ProviderStatus = "UNAVAILABLE",
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
