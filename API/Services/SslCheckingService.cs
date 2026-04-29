using SecurityAssessmentAPI.DTOs;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecurityAssessmentAPI.Services
{
    public interface ISslCheckingService
    {
        Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default);
        Task<SslDetailResult> GetSslDetailsAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class SslCheckingService : ISslCheckingService
    {
        private const int SslLabsMaxAttempts = 8;
        private const int ShortLivedCertificateMaxDays = 7;
        private static readonly TimeSpan SslLabsPollDelay = TimeSpan.FromSeconds(3);

        private readonly ISslLabsClient _sslLabsClient;
        private readonly ILogger<SslCheckingService> _logger;

        public SslCheckingService(
            ISslLabsClient sslLabsClient,
            ILogger<SslCheckingService> logger)
        {
            _sslLabsClient = sslLabsClient;
            _logger = logger;
        }

        public async Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default)
        {
            var detailResult = await GetSslDetailsAsync(domain, cancellationToken);
            return ToSummaryResult(detailResult);
        }

        public async Task<SslDetailResult> GetSslDetailsAsync(string domain, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SSL check started: {Domain}", domain);

            try
            {
                var sslLabsResponse = await WaitForSslLabsResultAsync(domain, cancellationToken);

                if (IsReadyStatus(sslLabsResponse.Status))
                {
                    var result = CreateSslLabsDetailResult(domain, sslLabsResponse);

                    _logger.LogInformation(
                        "SSL check completed (SSL Labs): Domain={Domain}, Score={Score}, Status={Status}",
                        domain,
                        result.OverallScore,
                        result.Status);

                    return result;
                }

                _logger.LogWarning(
                    "SSL Labs did not return a successful terminal result: Status={Status}, Domain={Domain}. The direct TLS fallback will be attempted.",
                    sslLabsResponse.Status,
                    domain);

                return await FallbackToDirectTlsOrErrorAsync(domain, sslLabsResponse.Status, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SSL Labs request failed or was interrupted: {Domain}. Falling back to direct TLS probe...", domain);
                return await FallbackToDirectTlsOrErrorAsync(domain, "SSL_LABS_UNAVAILABLE", cancellationToken, ex);
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

        private async Task<SslDetailResult> FallbackToDirectTlsOrErrorAsync(
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
                var totalValidityDays = (notAfter - notBefore).TotalDays;
                var remoteIpAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? string.Empty;
                var negotiatedCipherSuite = sslStream.NegotiatedCipherSuite;
                var cipherSuiteDescription = GetCipherSuiteDescription(negotiatedCipherSuite);

                var tlsScore = CalculateTlsScoreFromProtocol(sslStream.SslProtocol);
                var certificateValidityScore = notBefore <= now && notAfter > now ? 4 : 0;
                var remainingLifetimeScore = CalculateRemainingLifetimeScore(notBefore, notAfter);
                var cipherScore = CalculateCipherScoreFromSuite(negotiatedCipherSuite);

                var rawOverall = tlsScore + certificateValidityScore + remainingLifetimeScore + cipherScore;

                // Direct TLS probing is useful as a resilience fallback, but it is
                // lower-confidence than SSL Labs and should not produce
                // top-tier scores on its own.
                var overall = Math.Min(rawOverall, 24);
                var status = overall >= 15 ? "WARNING" : "FAIL";

                var result = new SslDetailResult
                {
                    Domain = domain,
                    OverallScore = overall,
                    Status = status,
                    DataSource = "DIRECT_TLS",
                    DataSourceStatus = sslLabsStatus ?? "UNKNOWN",
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
                            Details = GetRemainingLifetimeDetails(notBefore, notAfter, remainingDays, totalValidityDays)
                        },
                        CipherStrength = new SslScoreDetail
                        {
                            Score = cipherScore,
                            Details = $"Direct TLS probe observed cipher suite: {cipherSuiteDescription}."
                        }
                    },
                    Alerts = new List<SslAlert>(),
                    Endpoints = new List<SslEndpointDetail>
                    {
                        new SslEndpointDetail
                        {
                            IpAddress = remoteIpAddress,
                            ServerName = domain,
                            Grade = "UNAVAILABLE"
                        }
                    },
                    Certificate = CreateCertificateDetail(certificate, notBefore, notAfter),
                    SupportedTlsVersions = new List<string> { sslStream.SslProtocol.ToString() },
                    NotableCipherSuites = new List<string> { cipherSuiteDescription }
                };

                result.Alerts.Add(new SslAlert
                {
                    Type = "INFO",
                    Message = $"The result was produced by a direct TLS probe because SSL Labs was unavailable or did not return a usable result (Status={sslLabsStatus ?? "UNKNOWN"})."
                });

                if (certificateValidityScore == 0)
                {
                    result.Alerts.Add(new SslAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = "The certificate is invalid or expired."
                    });
                }
                else
                {
                    const bool renewalVerified = false;

                    if (remainingDays < 30 && !renewalVerified)
                    {
                        result.Alerts.Add(new SslAlert
                        {
                            Type = "CRITICAL_WARNING",
                            Message = "The certificate is approaching expiry and renewal has not been verified.",
                            ExpiryDate = notAfter.DateTime
                        });
                    }

                    if (remainingDays < 7 && !renewalVerified)
                    {
                        result.Alerts.Add(new SslAlert
                        {
                            Type = "CRITICAL_ALARM",
                            Message = "The certificate will expire very soon and renewal is not verified.",
                            ExpiryDate = notAfter.DateTime
                        });
                    }
                }

                EnsureAtLeastOneAlert(result);

                _logger.LogInformation("SSL check completed (direct TLS probe): Domain={Domain}, Score={Score}, Status={Status}", domain, result.OverallScore, result.Status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Direct TLS probe failed: {Domain}", domain);

                if (originalException != null)
                {
                    _logger.LogError(originalException, "Original SSL Labs failure before direct TLS probe: {Domain}", domain);
                }

                return CreateErrorResult(
                    domain,
                    $"SSL Labs did not return a usable result (Status={sslLabsStatus ?? "UNKNOWN"}), and the direct TLS probe also failed.");
            }
        }

        private SslDetailResult CreateSslLabsDetailResult(string domain, SslLabsResponse response)
        {
            var result = new SslDetailResult
            {
                Domain = domain,
                DataSource = "SSL_LABS",
                DataSourceStatus = response.Status
            };
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

            // Merge endpoint observations so one multi-IP host is scored as a single external-facing surface.
            var protocols = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Protocols ?? Enumerable.Empty<SslLabsProtocol>())
                .GroupBy(protocol => $"{protocol.Name}:{protocol.Version}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var suites = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Suites ?? Enumerable.Empty<SslLabsProtocolSuiteGroup>())
                .SelectMany(group => group.List ?? Enumerable.Empty<SslLabsSuite>())
                .ToList();

            result.Endpoints = response.Endpoints.Select(endpoint => new SslEndpointDetail
            {
                IpAddress = endpoint.IpAddress,
                ServerName = endpoint.ServerName,
                Grade = string.IsNullOrWhiteSpace(endpoint.Grade) ? "UNAVAILABLE" : endpoint.Grade
            }).ToList();

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
            result.Certificate = CreateCertificateDetail(cert);
            result.SupportedTlsVersions = GetSupportedTlsVersions(protocols);
            result.NotableCipherSuites = GetNotableCipherSuites(suites);

            AddAlerts(result, cert, response.Certs);

            if (!IsHttpsSupported(protocols))
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "INFO",
                    Message = "HTTPS protocol details were limited; the score was calculated from the available TLS data."
                });
            }

            EnsureAtLeastOneAlert(result);

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

            var versions = GetSupportedTlsVersions(protocols);

            return $"Supported TLS versions: {string.Join(", ", versions)}";
        }

        private string GetRemainingLifetimeDetails(SslLabsCert cert)
        {
            var notBefore = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotBefore);
            var notAfter = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter);
            var remainingDays = (notAfter - DateTimeOffset.Now).TotalDays;
            var totalValidityDays = (notAfter - notBefore).TotalDays;

            return GetRemainingLifetimeDetails(notBefore, notAfter, remainingDays, totalValidityDays);
        }

        private string GetRemainingLifetimeDetails(DateTimeOffset notBefore, DateTimeOffset notAfter, double remainingDays, double totalValidityDays)
        {
            if (totalValidityDays <= 0)
            {
                return "The certificate lifetime could not be determined.";
            }

            if (!IsShortLivedCertificate(totalValidityDays))
            {
                return $"Long-lived certificate: {remainingDays:F0} days remaining. Long-lived lifetime scoring is based on remaining days.";
            }

            var remainingPercentage = remainingDays <= 0 ? 0 : (remainingDays / totalValidityDays) * 100;
            return $"Short-lived certificate: {remainingPercentage:F0}% remaining ({remainingDays:F0} days out of {totalValidityDays:F0} days total lifetime).";
        }

        private string GetCipherDetails(List<SslLabsSuite> suites)
        {
            if (!suites.Any())
            {
                return "No cipher suite information was found.";
            }

            var strongestCipher = suites
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .OrderByDescending(s => s.CipherStrength)
                .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (strongestCipher == null)
            {
                return "Cipher suites were observed, but no notable cipher summary could be produced.";
            }

            return $"Strongest observed cipher: {strongestCipher.Name} ({strongestCipher.CipherStrength} bits). See notableCipherSuites for more.";
        }

        private void AddAlerts(SslDetailResult result, SslLabsCert cert, List<SslLabsCert> allCerts)
        {
            var remainingDays = (DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter) - DateTimeOffset.Now).TotalDays;
            var renewalVerified = IsSslLabsRenewalVerified(cert, allCerts);

            if (remainingDays < 30 && !renewalVerified)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "The certificate is approaching expiry and renewal has not been verified.",
                    ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter).DateTime
                });
            }

            if (remainingDays < 7 && !renewalVerified)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "CRITICAL_ALARM",
                    Message = "The certificate will expire very soon and renewal is not verified.",
                    ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter).DateTime
                });
            }

            if (remainingDays < 30 && renewalVerified)
            {
                result.Alerts.Add(new SslAlert
                {
                    Type = "INFO",
                    Message = "The certificate is close to expiry, but a replacement certificate appears to be provisioned.",
                    ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter).DateTime
                });
            }
        }

        private static bool IsSslLabsRenewalVerified(SslLabsCert current, List<SslLabsCert> allCerts)
        {
            if (allCerts.Count <= 1)
            {
                return false;
            }

            var currentNotAfter = DateTimeOffset.FromUnixTimeMilliseconds(current.NotAfter);
            var replacementWindowEnd = currentNotAfter.AddDays(14);
            var currentNames = GetCertificateNames(current);

            return allCerts.Any(candidate =>
            {
                if (ReferenceEquals(candidate, current))
                {
                    return false;
                }

                var candidateNotBefore = DateTimeOffset.FromUnixTimeMilliseconds(candidate.NotBefore);
                var candidateNotAfter = DateTimeOffset.FromUnixTimeMilliseconds(candidate.NotAfter);

                if (candidateNotAfter <= currentNotAfter || candidateNotBefore > replacementWindowEnd)
                {
                    return false;
                }

                var candidateNames = GetCertificateNames(candidate);
                if (currentNames.Count > 0 && candidateNames.Count > 0)
                {
                    return currentNames.Overlaps(candidateNames);
                }

                return SubjectLooksRelated(current.Subject, candidate.Subject);
            });
        }

        private static HashSet<string> GetCertificateNames(SslLabsCert cert)
        {
            return cert.CommonNames
                .Concat(cert.AltNames)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static bool SubjectLooksRelated(string? left, string? right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReadyStatus(string? status) =>
            string.Equals(status, "READY", StringComparison.OrdinalIgnoreCase);

        private static bool IsTerminalErrorStatus(string? status) =>
            string.Equals(status, "ERROR", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "DNS", StringComparison.OrdinalIgnoreCase);

        private static SslDetailResult CreateErrorResult(string domain, string message)
        {
            return new SslDetailResult
            {
                Domain = domain,
                Status = "ERROR",
                DataSource = "ERROR",
                Alerts = new List<SslAlert>
                {
                    new SslAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = message
                    }
                },
                Endpoints = new List<SslEndpointDetail>
                {
                    new SslEndpointDetail
                    {
                        ServerName = domain,
                        Grade = "UNAVAILABLE"
                    }
                }
            };
        }

        private static SslCheckResult ToSummaryResult(SslDetailResult detailResult)
        {
            return new SslCheckResult
            {
                Domain = detailResult.Domain,
                OverallScore = detailResult.OverallScore,
                MaxScore = detailResult.MaxScore,
                Status = detailResult.Status,
                Criteria = detailResult.Criteria,
                Alerts = detailResult.Alerts
            };
        }

        private static SslCertificateDetail CreateCertificateDetail(SslLabsCert cert)
        {
            var validFrom = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotBefore);
            var validUntil = DateTimeOffset.FromUnixTimeMilliseconds(cert.NotAfter);

            return new SslCertificateDetail
            {
                Subject = cert.Subject,
                Issuer = cert.IssuerSubject,
                FingerprintSha256 = cert.Sha256Hash,
                SignatureAlgorithm = cert.SignatureAlgorithm,
                Key = FormatCertificateKey(cert.KeyAlgorithm, cert.KeySize),
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                DaysRemaining = CalculateDaysRemaining(validUntil),
                CommonNames = cert.CommonNames,
                AltNames = cert.AltNames
            };
        }

        private static SslCertificateDetail CreateCertificateDetail(X509Certificate2 certificate, DateTimeOffset notBefore, DateTimeOffset notAfter)
        {
            return new SslCertificateDetail
            {
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                FingerprintSha256 = Convert.ToHexString(SHA256.HashData(certificate.RawData)).ToLowerInvariant(),
                SignatureAlgorithm = certificate.SignatureAlgorithm.FriendlyName ?? certificate.SignatureAlgorithm.Value ?? string.Empty,
                Key = GetCertificateKey(certificate),
                ValidFrom = notBefore,
                ValidUntil = notAfter,
                DaysRemaining = CalculateDaysRemaining(notAfter)
            };
        }

        private static string FormatCertificateKey(string keyAlgorithm, int? keySize)
        {
            var algorithm = string.IsNullOrWhiteSpace(keyAlgorithm) ? string.Empty : keyAlgorithm.Trim();
            if (keySize is > 0)
            {
                return string.IsNullOrWhiteSpace(algorithm)
                    ? $"{keySize} bits"
                    : $"{algorithm} {keySize} bits";
            }

            return algorithm;
        }

        private static string GetCertificateKey(X509Certificate2 certificate)
        {
            using var rsa = certificate.GetRSAPublicKey();
            if (rsa != null)
            {
                return $"RSA {rsa.KeySize} bits";
            }

            using var ecdsa = certificate.GetECDsaPublicKey();
            if (ecdsa != null)
            {
                return $"EC {ecdsa.KeySize} bits";
            }

            var friendlyName = certificate.PublicKey.Oid.FriendlyName ?? certificate.PublicKey.Oid.Value ?? string.Empty;
            var keyLength = certificate.PublicKey.EncodedKeyValue.RawData.Length * 8;
            return keyLength > 0 && !string.IsNullOrWhiteSpace(friendlyName)
                ? $"{friendlyName} {keyLength} bits"
                : friendlyName;
        }

        private static List<string> GetSupportedTlsVersions(List<SslLabsProtocol> protocols)
        {
            return protocols
                .Select(protocol => string.IsNullOrWhiteSpace(protocol.Name) ? protocol.Version : $"{protocol.Name} {protocol.Version}")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(GetTlsVersionSortKey)
                .ThenByDescending(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> GetNotableCipherSuites(List<SslLabsSuite> suites)
        {
            return suites
                .Where(suite => !string.IsNullOrWhiteSpace(suite.Name))
                .OrderByDescending(suite => suite.CipherStrength)
                .ThenBy(suite => suite.Name, StringComparer.OrdinalIgnoreCase)
                .Select(suite => $"{suite.Name} ({suite.CipherStrength} bits)")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }

        private static int CalculateDaysRemaining(DateTimeOffset validUntil)
        {
            var remainingDays = (validUntil - DateTimeOffset.UtcNow).TotalDays;
            return remainingDays >= 0
                ? (int)Math.Ceiling(remainingDays)
                : (int)Math.Floor(remainingDays);
        }

        private static double GetTlsVersionSortKey(string value)
        {
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var versionText = parts.Length == 0 ? value : parts[^1];

            return double.TryParse(versionText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : double.MinValue;
        }

        private static void EnsureAtLeastOneAlert(SslDetailResult result)
        {
            if (result.Alerts.Count > 0)
            {
                return;
            }

            result.Alerts.Add(new SslAlert
            {
                Type = "INFO",
                Message = "No important SSL/TLS alerts were detected."
            });
        }

        private static int CalculateRemainingLifetimeScore(DateTimeOffset notBefore, DateTimeOffset notAfter)
        {
            var totalDays = (notAfter - notBefore).TotalDays;
            var remainingDays = (notAfter - DateTimeOffset.UtcNow).TotalDays;
            if (totalDays <= 0)
            {
                return 0;
            }

            if (!IsShortLivedCertificate(totalDays))
            {
                if (remainingDays >= 30) return 6;
                if (remainingDays >= 1) return 3;
                return 0;
            }

            var percentage = (remainingDays / totalDays) * 100;

            if (percentage >= 50) return 6;
            if (percentage >= 30) return 4;
            if (percentage >= 1) return 2;
            return 0;
        }

        private static bool IsShortLivedCertificate(double totalValidityDays) =>
            totalValidityDays > 0 && totalValidityDays <= ShortLivedCertificateMaxDays;

        private static int CalculateTlsScoreFromProtocol(SslProtocols protocol) =>
            protocol switch
            {
                SslProtocols.Tls13 => 10,
                SslProtocols.Tls12 => 7,
                _ when protocol != SslProtocols.None => 4,
                _ => 0
            };

        private static int CalculateCipherScoreFromSuite(TlsCipherSuite cipherSuite)
        {
            if (cipherSuite == TlsCipherSuite.TLS_AES_256_GCM_SHA384 ||
                cipherSuite == TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256 ||
                cipherSuite == TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 ||
                cipherSuite == TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 ||
                cipherSuite == TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384)
            {
                return 10;
            }

            if (cipherSuite == TlsCipherSuite.TLS_AES_128_GCM_SHA256 ||
                cipherSuite == TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 ||
                cipherSuite == TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 ||
                cipherSuite == TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256)
            {
                return 7;
            }

            return cipherSuite != 0 ? 4 : 0;
        }

        private static string GetCipherSuiteDescription(TlsCipherSuite cipherSuite)
        {
            return cipherSuite != 0
                ? cipherSuite.ToString()
                : "Unknown cipher suite";
        }
    }
}
