using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.Services
{
    public interface IAssessmentCheckingService
    {
        Task<AssessmentCheckResult> CheckAssessmentAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class AssessmentCheckingService : IAssessmentCheckingService
    {
        private const decimal SslWeightWithEmail = 35m;
        private const decimal HeadersWeightWithEmail = 25m;
        private const decimal EmailWeightWithEmail = 20m;
        private const decimal ReputationWeightWithEmail = 20m;

        private const decimal SslWeightWithoutEmail = 50m;
        private const decimal HeadersWeightWithoutEmail = 35m;
        private const decimal ReputationWeightWithoutEmail = 15m;

        private readonly ISslCheckingService _sslCheckingService;
        private readonly IHeadersCheckingService _headersCheckingService;
        private readonly IEmailCheckingService _emailCheckingService;
        private readonly IReputationCheckingService _reputationCheckingService;
        private readonly IPqcCheckingService _pqcCheckingService;
        private readonly ILogger<AssessmentCheckingService> _logger;

        public AssessmentCheckingService(
            ISslCheckingService sslCheckingService,
            IHeadersCheckingService headersCheckingService,
            IEmailCheckingService emailCheckingService,
            IReputationCheckingService reputationCheckingService,
            IPqcCheckingService pqcCheckingService,
            ILogger<AssessmentCheckingService> logger)
        {
            _sslCheckingService = sslCheckingService;
            _headersCheckingService = headersCheckingService;
            _emailCheckingService = emailCheckingService;
            _reputationCheckingService = reputationCheckingService;
            _pqcCheckingService = pqcCheckingService;
            _logger = logger;
        }

        public async Task<AssessmentCheckResult> CheckAssessmentAsync(string domain, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Final assessment started: {Domain}", domain);

            var sslTask = _sslCheckingService.CheckSslAsync(domain, cancellationToken);
            var headersTask = _headersCheckingService.CheckHeadersAsync(domain, cancellationToken);
            var emailTask = _emailCheckingService.CheckEmailAsync(domain, cancellationToken);
            var reputationTask = _reputationCheckingService.CheckReputationAsync(domain, cancellationToken);
            var pqcTask = _pqcCheckingService.CheckPqcAsync(domain, cancellationToken);

            await Task.WhenAll(sslTask, headersTask, emailTask, reputationTask, pqcTask);

            var sslResult = await sslTask;
            var headersResult = await headersTask;
            var emailResult = await emailTask;
            var reputationResult = await reputationTask;
            var pqcResult = await pqcTask;

            var emailIncluded = emailResult.ModuleApplicable && emailResult.HasMailService;

            var result = new AssessmentCheckResult
            {
                Domain = NormalizeDomain(domain),
                EmailModuleIncluded = emailIncluded,
                PqcReadiness = pqcResult
            };

            ApplyWeights(result, emailIncluded);
            PopulateModuleScores(result, sslResult, headersResult, emailResult, reputationResult, emailIncluded);

            var total = result.Modules.SslTls.WeightedContribution +
                        result.Modules.HttpHeaders.WeightedContribution +
                        result.Modules.EmailSecurity.WeightedContribution +
                        result.Modules.Reputation.WeightedContribution;

            result.OverallScore = (int)Math.Round(total, MidpointRounding.AwayFromZero);
            result.Status = GetAssessmentStatus(result.OverallScore, sslResult, headersResult, emailResult, reputationResult);
            result.Grade = GetGrade(result.OverallScore);

            AddAlerts(result, sslResult, headersResult, emailResult, reputationResult);

            _logger.LogInformation("Final assessment completed: Domain={Domain}, Score={Score}, Grade={Grade}, Status={Status}",
                result.Domain, result.OverallScore, result.Grade, result.Status);

            return result;
        }

        private static void ApplyWeights(AssessmentCheckResult result, bool emailIncluded)
        {
            result.Weights.SslTls = emailIncluded ? SslWeightWithEmail : SslWeightWithoutEmail;
            result.Weights.HttpHeaders = emailIncluded ? HeadersWeightWithEmail : HeadersWeightWithoutEmail;
            result.Weights.EmailSecurity = emailIncluded ? EmailWeightWithEmail : 0m;
            result.Weights.Reputation = emailIncluded ? ReputationWeightWithEmail : ReputationWeightWithoutEmail;
        }

        private static void PopulateModuleScores(
            AssessmentCheckResult result,
            SslCheckResult sslResult,
            HeadersCheckResult headersResult,
            EmailCheckResult emailResult,
            ReputationCheckResult reputationResult,
            bool emailIncluded)
        {
            result.Modules.SslTls = CreateModuleScore(true, result.Weights.SslTls, sslResult.OverallScore, sslResult.MaxScore, sslResult.Status);
            result.Modules.HttpHeaders = CreateModuleScore(true, result.Weights.HttpHeaders, headersResult.OverallScore, headersResult.MaxScore, headersResult.Status);
            result.Modules.Reputation = CreateModuleScore(true, result.Weights.Reputation, reputationResult.OverallScore, reputationResult.MaxScore, reputationResult.Status);
            result.Modules.EmailSecurity = emailIncluded
                ? CreateModuleScore(true, result.Weights.EmailSecurity, emailResult.OverallScore, emailResult.MaxScore, emailResult.Status)
                : CreateModuleScore(false, 0m, emailResult.OverallScore, emailResult.MaxScore, emailResult.Status);
        }

        private static AssessmentModuleScore CreateModuleScore(bool included, decimal weightPercent, int rawScore, int rawMaxScore, string status)
        {
            var normalized = rawMaxScore > 0 ? (decimal)rawScore / rawMaxScore * 100m : 0m;

            return new AssessmentModuleScore
            {
                Included = included,
                WeightPercent = weightPercent,
                RawScore = rawScore,
                RawMaxScore = rawMaxScore,
                NormalizedScore = Math.Round(normalized, 2),
                WeightedContribution = included ? Math.Round(normalized * (weightPercent / 100m), 2) : 0m,
                Status = status
            };
        }

        private static string GetAssessmentStatus(
            int overallScore,
            SslCheckResult sslResult,
            HeadersCheckResult headersResult,
            EmailCheckResult emailResult,
            ReputationCheckResult reputationResult)
        {
            if (string.Equals(sslResult.Status, "FAIL", StringComparison.OrdinalIgnoreCase) && sslResult.OverallScore == 0)
            {
                return "FAIL";
            }

            if (string.Equals(sslResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(headersResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(reputationResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return "PARTIAL";
            }

            if (emailResult.ModuleApplicable && string.Equals(emailResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return "PARTIAL";
            }

            if (overallScore >= 80) return "PASS";
            if (overallScore >= 50) return "WARNING";
            return "FAIL";
        }

        private static string GetGrade(int score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            if (score >= 50) return "E";
            return "F";
        }

        private static void AddAlerts(
            AssessmentCheckResult result,
            SslCheckResult sslResult,
            HeadersCheckResult headersResult,
            EmailCheckResult emailResult,
            ReputationCheckResult reputationResult)
        {
            if (!result.EmailModuleIncluded)
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "INFO",
                    Message = "No MX record was found. The email security module was excluded and the final score was normalized according to the no-email model."
                });
            }

            if (string.Equals(sslResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "SSL/TLS analysis could not be completed reliably. The final assessment is therefore partial."
                });
            }
            else if (sslResult.Alerts.Any(alert => string.Equals(alert.Type, "CRITICAL_ALARM", StringComparison.OrdinalIgnoreCase)))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "CRITICAL_ALARM",
                    Message = "Critical SSL/TLS findings were detected and heavily affect the final assessment."
                });
            }
            else if (string.Equals(sslResult.Status, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "Important SSL/TLS weaknesses were detected."
                });
            }
            else if (sslResult.Alerts.Any(alert => string.Equals(alert.Type, "CRITICAL_WARNING", StringComparison.OrdinalIgnoreCase)))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "SSL/TLS analysis identified certificate lifecycle or configuration warnings."
                });
            }
            else if (string.Equals(sslResult.Status, "WARNING", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "SSL/TLS analysis identified moderate weaknesses."
                });
            }

            if (string.Equals(headersResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "HTTP security header analysis could not be completed reliably."
                });
            }
            else if (string.Equals(headersResult.Status, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "Important HTTP security header protections are missing."
                });
            }
            else if (string.Equals(headersResult.Status, "WARNING", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "Important HTTP security header weaknesses were detected."
                });
            }

            if (result.EmailModuleIncluded)
            {
                if (string.Equals(emailResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    result.Alerts.Add(new AssessmentAlert
                    {
                        Type = "WARNING",
                        Message = "Email security analysis could not be completed reliably."
                    });
                }
                else if (string.Equals(emailResult.Status, "FAIL", StringComparison.OrdinalIgnoreCase))
                {
                    result.Alerts.Add(new AssessmentAlert
                    {
                        Type = "CRITICAL_WARNING",
                        Message = "Important email security weaknesses were detected."
                    });
                }
                else if (string.Equals(emailResult.Status, "WARNING", StringComparison.OrdinalIgnoreCase))
                {
                    result.Alerts.Add(new AssessmentAlert
                    {
                        Type = "WARNING",
                        Message = "Email security analysis identified moderate weaknesses."
                    });
                }
            }

            if (string.Equals(reputationResult.Status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "Domain/IP reputation analysis could not be completed reliably."
                });
            }
            else if (string.Equals(reputationResult.Status, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "Reputation analysis indicates elevated risk signals."
                });
            }
            else if (string.Equals(reputationResult.Status, "WARNING", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new AssessmentAlert
                {
                    Type = "WARNING",
                    Message = "Reputation analysis identified moderate risk signals."
                });
            }
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
    }
}
