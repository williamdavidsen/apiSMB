using SecurityAssessmentAPI.DTOs;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SecurityAssessmentAPI.Services
{
    public interface ISslCheckingService
    {
        Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class SslCheckingService : ISslCheckingService
    {
        private const int SslLabsMaxAttempts = 8;
        private static readonly TimeSpan SslLabsPollDelay = TimeSpan.FromSeconds(3);

        private readonly ISslLabsClient _sslLabsClient;
        private readonly IHardenizeClient _hardenizeClient;
        private readonly ILogger<SslCheckingService> _logger;

        public SslCheckingService(
            ISslLabsClient sslLabsClient,
            IHardenizeClient hardenizeClient,
            ILogger<SslCheckingService> logger)
        {
            _sslLabsClient = sslLabsClient;
            _hardenizeClient = hardenizeClient;
            _logger = logger;
        }

        public async Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SSL check started: {Domain}", domain);

            try
            {
                var sslLabsResponse = await WaitForSslLabsResultAsync(domain, cancellationToken);

                if (IsReadyStatus(sslLabsResponse.Status))
                {
                    var result = CalculateScore(sslLabsResponse);
                    result.Domain = domain;

                    _logger.LogInformation(
                        "SSL check completed (SSL Labs): Domain={Domain}, Score={Score}, Status={Status}",
                        domain,
                        result.OverallScore,
                        result.Status);

                    return result;
                }

                _logger.LogWarning(
                    "SSL Labs did not return a successful terminal result: Status={Status}, Domain={Domain}. The Hardenize fallback will be attempted.",
                    sslLabsResponse.Status,
                    domain);

                return await FallbackToHardenizeOrErrorAsync(domain, sslLabsResponse.Status, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SSL Labs request failed or was interrupted: {Domain}. Falling back to Hardenize...", domain);
                return await FallbackToHardenizeOrErrorAsync(domain, "SSL_LABS_UNAVAILABLE", cancellationToken, ex);
            }
        }

        private async Task<SslLabsResponse> WaitForSslLabsResultAsync(string domain, CancellationToken cancellationToken)
        {
            SslLabsResponse? latestResponse = null;

            // SSL Labs is asynchronous, so poll until a usable terminal state is returned or the retry budget is exhausted.
            for (var attempt = 1; attempt <= SslLabsMaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                latestResponse = await _sslLabsClient.AnalyzeAsync(domain, cancellationToken);
                if (IsReadyStatus(latestResponse.Status) || IsTerminalErrorStatus(latestResponse.Status))
                {
                    return latestResponse;
                }

                if (attempt < SslLabsMaxAttempts)
                {
                    _logger.LogInformation(
                        "SSL Labs analysis is not ready yet: Domain={Domain}, Status={Status}, Attempt={Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds.",
                        domain,
                        latestResponse.Status,
                        attempt,
                        SslLabsMaxAttempts,
                        SslLabsPollDelay.TotalSeconds);

                    await Task.Delay(SslLabsPollDelay, cancellationToken);
                }
            }

            return latestResponse ?? new SslLabsResponse { Host = domain, Status = "ERROR" };
        }

        private async Task<SslCheckResult> FallbackToHardenizeOrErrorAsync(
            string domain,
            string? sslLabsStatus,
            CancellationToken cancellationToken,
            Exception? originalException = null)
        {
            // Hardenize keeps the assessment resilient when SSL Labs is unavailable, even though the dataset is less detailed.
            var hardenizeResponse = await _hardenizeClient.GetCertificateDiscoveryAsync(domain, cancellationToken);
            if (hardenizeResponse == null || hardenizeResponse.Records == null || !hardenizeResponse.Records.Any())
            {
                _logger.LogWarning("No data was returned from Hardenize: {Domain}", domain);

                if (originalException != null)
                {
                    _logger.LogError(originalException, "SSL check error: {Domain}", domain);
                }

                return await FallbackToDirectTlsOrErrorAsync(domain, sslLabsStatus, cancellationToken, originalException);
            }

            var record = hardenizeResponse.Records.First();
            var now = DateTimeOffset.Now;
            var validFrom = record.valid_from.HasValue ? DateTimeOffset.FromUnixTimeSeconds(record.valid_from.Value) : now;
            var validTo = record.valid_until.HasValue ? DateTimeOffset.FromUnixTimeSeconds(record.valid_until.Value) : now;
            var remainingDays = (validTo - now).TotalDays;

            var certificateValidity = validFrom <= now && remainingDays > 0;
            var tlsScore = 7;
            var validDaysScore = remainingDays > 365 ? 6 : remainingDays > 180 ? 4 : remainingDays > 30 ? 2 : 0;
            var cipherScore = 6;

            var overall = tlsScore + (certificateValidity ? 4 : 0) + validDaysScore + cipherScore;
            var status = overall >= 25 ? "PASS" : overall >= 15 ? "WARNING" : "FAIL";

            var result = new SslCheckResult
            {
                Domain = domain,
                OverallScore = overall,
                Status = status,
                Criteria = new SslCriteria
                {
                    TlsVersion = new SslScoreDetail { Score = tlsScore, Details = "Limited TLS data was received from Hardenize." },
                    CertificateValidity = new SslScoreDetail
                    {
                        Score = certificateValidity ? 4 : 0,
                        Details = certificateValidity ? "The certificate is valid." : "The certificate has expired or is missing."
                    },
                    RemainingLifetime = new SslScoreDetail
                    {
                        Score = validDaysScore,
                        Details = validDaysScore > 0 ? $"Remaining lifetime: {remainingDays:F0} days" : "The certificate has expired or its lifetime is unknown."
                    },
                    CipherStrength = new SslScoreDetail { Score = cipherScore, Details = "A default cipher score was used because cipher data was limited." }
                },
                Alerts = new List<SslAlert>()
            };

            if (!certificateValidity)
            {
                result.Alerts.Add(new SslAlert { Type = "CRITICAL_ALARM", Message = "The certificate is invalid or expired (Hardenize)." });
            }
            else if (remainingDays < 30)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "The certificate will expire very soon (Hardenize).",
                    ExpiryDate = validTo.DateTime
                });
            }

            _logger.LogInformation("SSL check completed (Hardenize): Domain={Domain}, Score={Score}, Status={Status}", domain, result.OverallScore, result.Status);
            return result;
        }

        private async Task<SslCheckResult> FallbackToDirectTlsOrErrorAsync(
            string domain,
            string? sslLabsStatus,
            CancellationToken cancellationToken,
            Exception? originalException = null)
        {
            // A direct TLS handshake is the last fallback and only confirms what can be observed live from the endpoint itself.
            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(domain, 443, cancellationToken);

                using var sslStream = new SslStream(
                    tcpClient.GetStream(),
                    leaveInnerStreamOpen: false,
                    (_, _, _, _) => true);

                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = domain,
                    EnabledSslProtocols = SslProtocols.None,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, cancellationToken);

                var remoteCertificate = sslStream.RemoteCertificate;
                if (remoteCertificate == null)
                {
                    return CreateErrorResult(domain, "The TLS handshake succeeded, but no remote certificate was available.");
                }

                var certificate = remoteCertificate as X509Certificate2 ?? new X509Certificate2(remoteCertificate);
                var now = DateTimeOffset.UtcNow;
                var notBefore = new DateTimeOffset(certificate.NotBefore.ToUniversalTime());
                var notAfter = new DateTimeOffset(certificate.NotAfter.ToUniversalTime());
                var remainingDays = (notAfter - now).TotalDays;

                var tlsScore = CalculateTlsScoreFromProtocol(sslStream.SslProtocol);
                var certificateValidityScore = notBefore <= now && notAfter > now ? 4 : 0;
                var remainingLifetimeScore = CalculateRemainingLifetimeScore(notBefore, notAfter);
                var cipherScore = CalculateCipherScoreFromStrength(sslStream.CipherStrength);

                var rawOverall = tlsScore + certificateValidityScore + remainingLifetimeScore + cipherScore;

                // Direct TLS probing is useful as a resilience fallback, but it is
                // lower-confidence than SSL Labs/Hardenize and should not produce
                // top-tier scores on its own.
                var overall = Math.Min(rawOverall, 24);
                var status = overall >= 15 ? "WARNING" : "FAIL";

                var result = new SslCheckResult
                {
                    Domain = domain,
                    OverallScore = overall,
                    Status = status,
                    Criteria = new SslCriteria
                    {
                        TlsVersion = new SslScoreDetail
                        {
                            Score = tlsScore,
                            Details = $"Direct TLS probe observed protocol: {sslStream.SslProtocol}."
                        },
                        CertificateValidity = new SslScoreDetail
                        {
                            Score = certificateValidityScore,
                            Details = certificateValidityScore > 0 ? "The certificate is valid." : "The certificate is invalid or expired."
                        },
                        RemainingLifetime = new SslScoreDetail
                        {
                            Score = remainingLifetimeScore,
                            Details = $"The certificate will expire in {remainingDays:F0} days."
                        },
                        CipherStrength = new SslScoreDetail
                        {
                            Score = cipherScore,
                            Details = $"Direct TLS probe observed cipher strength: {sslStream.CipherStrength} bits."
                        }
                    },
                    Alerts = new List<SslAlert>()
                };

                result.Alerts.Add(new SslAlert
                {
                    Type = "INFO",
                    Message = $"The result was produced by a direct TLS probe because SSL Labs (Status={sslLabsStatus ?? "UNKNOWN"}) and Hardenize data were unavailable."
                });

                if (certificateValidityScore == 0)
                {
                    result.Alerts.Add(new SslAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = "The certificate is invalid or expired."
                    });
                }
                else if (remainingDays < 30)
                {
                    result.Alerts.Add(new SslAlert
                    {
                        Type = "CRITICAL_WARNING",
                        Message = "The certificate will expire soon.",
                        ExpiryDate = notAfter.DateTime
                    });
                }

                _logger.LogInformation("SSL check completed (direct TLS probe): Domain={Domain}, Score={Score}, Status={Status}", domain, result.OverallScore, result.Status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Direct TLS probe failed: {Domain}", domain);

                if (originalException != null)
                {
                    _logger.LogError(originalException, "Original SSL Labs/Hardenize failure before direct TLS probe: {Domain}", domain);
                }

                return CreateErrorResult(
                    domain,
                    $"SSL Labs did not return a usable result (Status={sslLabsStatus ?? "UNKNOWN"}), Hardenize returned no data, and the direct TLS probe also failed.");
            }
        }

        private SslCheckResult CalculateScore(SslLabsResponse response)
        {
            var result = new SslCheckResult();
            var cert = response.Certs.FirstOrDefault();

            if (!response.Endpoints.Any())
            {
                result.Status = "FAIL";
                result.Alerts.Add(new SslAlert { Type = "CRITICAL_ALARM", Message = "No HTTPS endpoint was found." });
                return result;
            }

            if (cert == null)
            {
                result.Status = "FAIL";
                result.Alerts.Add(new SslAlert { Type = "CRITICAL_ALARM", Message = "No certificate information was returned by SSL Labs." });
                return result;
            }

            if (IsCertificateExpired(cert))
            {
                result.OverallScore = 0;
                result.Status = "FAIL";
                result.Alerts.Add(new SslAlert { Type = "CRITICAL_ALARM", Message = "The SSL/TLS certificate has expired." });
                return result;
            }

            // Merge endpoint observations so one multi-IP host is scored as a single customer-facing surface.
            var protocols = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Protocols ?? Enumerable.Empty<SslLabsProtocol>())
                .GroupBy(protocol => $"{protocol.Name}:{protocol.Version}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var suites = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Suites ?? Enumerable.Empty<SslLabsProtocolSuiteGroup>())
                .SelectMany(group => group.List ?? Enumerable.Empty<SslLabsSuite>())
                .ToList();

            result.Criteria.TlsVersion.Score = CalculateTlsScore(protocols);
            result.Criteria.CertificateValidity.Score = IsCertificateValid(cert) ? 4 : 0;
            result.Criteria.RemainingLifetime.Score = CalculateRemainingLifetimeScore(cert);
            result.Criteria.CipherStrength.Score = CalculateCipherScore(suites);

            result.OverallScore =
                result.Criteria.TlsVersion.Score +
                result.Criteria.CertificateValidity.Score +
                result.Criteria.RemainingLifetime.Score +
                result.Criteria.CipherStrength.Score;

            result.Status = result.OverallScore >= 25 ? "PASS" : result.OverallScore >= 15 ? "WARNING" : "FAIL";

            result.Criteria.TlsVersion.Details = GetTlsDetails(protocols);
            result.Criteria.CertificateValidity.Details = IsCertificateValid(cert) ? "Valid" : "Invalid";
            result.Criteria.RemainingLifetime.Details = GetRemainingLifetimeDetails(cert);
            result.Criteria.CipherStrength.Details = GetCipherDetails(suites);

            AddAlerts(result, cert);

            if (!IsHttpsSupported(protocols))
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "INFO",
                    Message = "HTTPS protocol details were limited; the score was calculated from the available TLS data."
                });
            }

            return result;
        }

        private bool IsHttpsSupported(List<SslLabsProtocol> protocols) => protocols.Any();

        private bool IsCertificateExpired(SslLabsCert cert) =>
            DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter) < DateTimeOffset.Now;

        private bool IsCertificateValid(SslLabsCert cert) =>
            !IsCertificateExpired(cert) && DateTimeOffset.FromUnixTimeMilliseconds(cert.NotBefore) < DateTimeOffset.Now;

        private int CalculateTlsScore(List<SslLabsProtocol> protocols)
        {
            if (protocols.Any(p => p.Name.Equals("TLS", StringComparison.OrdinalIgnoreCase) && p.Version == "1.3")) return 10;
            if (protocols.Any(p => p.Name.Equals("TLS", StringComparison.OrdinalIgnoreCase) && p.Version == "1.2")) return 7;
            if (protocols.Any(p => p.Name.Equals("TLS", StringComparison.OrdinalIgnoreCase) && p.Version == "1.1")) return 4;
            return 0;
        }

        private int CalculateRemainingLifetimeScore(SslLabsCert cert)
        {
            return CalculateRemainingLifetimeScore(
                DateTimeOffset.FromUnixTimeMilliseconds(cert.NotBefore),
                DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter));
        }

        private int CalculateCipherScore(List<SslLabsSuite> suites)
        {
            if (suites.Any(s => s.CipherStrength >= 256 ||
                                s.Name.Contains("CHACHA20", StringComparison.OrdinalIgnoreCase))) return 10;
            if (suites.Any(s => s.CipherStrength >= 128 ||
                                s.Name.Contains("AES_128", StringComparison.OrdinalIgnoreCase))) return 7;
            return suites.Any() ? 4 : 0;
        }

        private string GetTlsDetails(List<SslLabsProtocol> protocols)
        {
            if (!protocols.Any())
            {
                return "No TLS protocol information was found.";
            }

            var versions = protocols
                .Select(p => string.IsNullOrWhiteSpace(p.Name) ? p.Version : $"{p.Name} {p.Version}")
                .Distinct();

            return $"Supported TLS versions: {string.Join(", ", versions)}";
        }

        private string GetRemainingLifetimeDetails(SslLabsCert cert)
        {
            var remainingDays = (DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter) - DateTimeOffset.Now).TotalDays;
            return $"The certificate will expire in {remainingDays:F0} days.";
        }

        private string GetCipherDetails(List<SslLabsSuite> suites)
        {
            if (!suites.Any())
            {
                return "No cipher suite information was found.";
            }

            var strongCiphers = suites
                .Where(s => s.Name.Contains("AES", StringComparison.OrdinalIgnoreCase) ||
                            s.Name.Contains("CHACHA", StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .Select(s => s.Name);

            return $"Strong ciphers: {string.Join(", ", strongCiphers)}";
        }

        private void AddAlerts(SslCheckResult result, SslLabsCert cert)
        {
            var remainingDays = (DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter) - DateTimeOffset.Now).TotalDays;

            if (remainingDays < 30)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "The certificate will expire soon.",
                    ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter).DateTime
                });
            }

            if (remainingDays < 7)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "CRITICAL_ALARM",
                    Message = "The certificate will expire very soon and must be renewed.",
                    ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter).DateTime
                });
            }
        }

        private static bool IsReadyStatus(string? status) =>
            string.Equals(status, "READY", StringComparison.OrdinalIgnoreCase);

        private static bool IsTerminalErrorStatus(string? status) =>
            string.Equals(status, "ERROR", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "DNS", StringComparison.OrdinalIgnoreCase);

        private static SslCheckResult CreateErrorResult(string domain, string message)
        {
            return new SslCheckResult
            {
                Domain = domain,
                Status = "ERROR",
                Alerts = new List<SslAlert>
                {
                    new SslAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = message
                    }
                }
            };
        }

        private static int CalculateRemainingLifetimeScore(DateTimeOffset notBefore, DateTimeOffset notAfter)
        {
            var totalDays = (notAfter - notBefore).TotalDays;
            var remainingDays = (notAfter - DateTimeOffset.UtcNow).TotalDays;
            if (totalDays <= 0)
            {
                return 0;
            }

            var percentage = (remainingDays / totalDays) * 100;

            if (percentage > 90) return 6;
            if (percentage > 50) return 4;
            if (percentage > 10) return 2;
            return 0;
        }

        private static int CalculateTlsScoreFromProtocol(SslProtocols protocol) =>
            protocol switch
            {
                SslProtocols.Tls13 => 10,
                SslProtocols.Tls12 => 7,
                SslProtocols.Tls11 => 4,
                _ => 0
            };

        private static int CalculateCipherScoreFromStrength(int cipherStrength)
        {
            if (cipherStrength >= 256) return 10;
            if (cipherStrength >= 128) return 7;
            return cipherStrength > 0 ? 4 : 0;
        }
    }
}
