using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.Services
{
    public interface IPqcCheckingService
    {
        Task<PqcCheckResult> CheckPqcAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class PqcCheckingService : IPqcCheckingService
    {
        private const int SslLabsMaxAttempts = 8;
        private static readonly TimeSpan SslLabsPollDelay = TimeSpan.FromSeconds(3);

        private static readonly string[] ExplicitPqcKeywords =
        {
            "MLKEM", "KYBER", "HQC", "BIKE", "NTRU", "FRODO", "X25519MLKEM", "SECP256R1MLKEM"
        };

        private readonly ISslLabsClient _sslLabsClient;
        private readonly ILogger<PqcCheckingService> _logger;

        public PqcCheckingService(ISslLabsClient sslLabsClient, ILogger<PqcCheckingService> logger)
        {
            _sslLabsClient = sslLabsClient;
            _logger = logger;
        }

        public async Task<PqcCheckResult> CheckPqcAsync(string domain, CancellationToken cancellationToken = default)
        {
            var normalizedDomain = NormalizeDomain(domain);
            _logger.LogInformation("PQC check started: {Domain}", normalizedDomain);

            try
            {
                // PQC is reported as an evidence-based readiness signal, not as a hard compliance claim.
                var sslLabsResponse = await WaitForSslLabsResultAsync(normalizedDomain, cancellationToken);
                return ClassifyPqcReadiness(normalizedDomain, sslLabsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PQC check failed: {Domain}", normalizedDomain);
                return new PqcCheckResult
                {
                    Domain = normalizedDomain,
                    PqcDetected = false,
                    Status = "UNKNOWN",
                    Mode = "Unclear",
                    ReadinessLevel = "Unknown / not verifiable",
                    AlgorithmFamily = "Unknown",
                    HandshakeSupported = false,
                    Confidence = "LOW",
                    Notes = "PQC readiness could not be reliably determined because TLS analysis data was unavailable."
                };
            }
        }

        private async Task<SslLabsResponse> WaitForSslLabsResultAsync(string domain, CancellationToken cancellationToken)
        {
            SslLabsResponse? latestResponse = null;

            for (var attempt = 1; attempt <= SslLabsMaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                latestResponse = await _sslLabsClient.AnalyzeAsync(domain, cancellationToken);
                if (string.Equals(latestResponse.Status, "READY", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(latestResponse.Status, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(latestResponse.Status, "DNS", StringComparison.OrdinalIgnoreCase))
                {
                    return latestResponse;
                }

                if (attempt < SslLabsMaxAttempts)
                {
                    await Task.Delay(SslLabsPollDelay, cancellationToken);
                }
            }

            return latestResponse ?? new SslLabsResponse { Host = domain, Status = "ERROR" };
        }

        private PqcCheckResult ClassifyPqcReadiness(string domain, SslLabsResponse response)
        {
            var protocols = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Protocols ?? Enumerable.Empty<SslLabsProtocol>())
                .ToList();

            var suites = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.Suites ?? Enumerable.Empty<SslLabsProtocolSuiteGroup>())
                .SelectMany(group => group.List ?? Enumerable.Empty<SslLabsSuite>())
                .ToList();

            var namedGroups = response.Endpoints
                .SelectMany(endpoint => endpoint.Details?.NamedGroups ?? Enumerable.Empty<SslLabsNamedGroup>())
                .ToList();

            var evidence = suites
                .SelectMany(suite => new[] { suite.Name, suite.NamedGroupName })
                .Concat(namedGroups.Select(group => group.Name))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Look only for explicit hybrid/PQC identifiers; modern classical groups alone are not enough to overclaim readiness.
            var pqcEvidence = evidence
                .Where(value => ExplicitPqcKeywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var supportsTls13 = protocols.Any(p => p.Name.Equals("TLS", StringComparison.OrdinalIgnoreCase) && p.Version == "1.3");
            var hasModernNamedGroups = evidence.Any(value =>
                value.Contains("X25519", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("SECP256R1", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("SECP384R1", StringComparison.OrdinalIgnoreCase));

            if (pqcEvidence.Any())
            {
                return new PqcCheckResult
                {
                    Domain = domain,
                    PqcDetected = true,
                    Status = "INFO",
                    Mode = "Hybrid key exchange",
                    ReadinessLevel = "Hybrid PQC supported",
                    AlgorithmFamily = DetermineAlgorithmFamily(pqcEvidence),
                    HandshakeSupported = true,
                    Confidence = "MEDIUM",
                    Notes = "Explicit hybrid or post-quantum TLS indicators were observed in the TLS analysis data.",
                    Evidence = pqcEvidence
                };
            }

            if (supportsTls13 && hasModernNamedGroups)
            {
                var modernEvidence = evidence
                    .Where(value => value.Contains("X25519", StringComparison.OrdinalIgnoreCase) ||
                                    value.Contains("SECP256R1", StringComparison.OrdinalIgnoreCase) ||
                                    value.Contains("SECP384R1", StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new PqcCheckResult
                {
                    Domain = domain,
                    PqcDetected = false,
                    Status = "INFO",
                    Mode = "Classical TLS with modern groups",
                    ReadinessLevel = "Unknown / not verifiable",
                    AlgorithmFamily = "Classical elliptic-curve groups",
                    HandshakeSupported = true,
                    Confidence = "LOW",
                    Notes = "No explicit PQC support was detected. Modern classical TLS groups were observed, but these are not sufficient to verify post-quantum readiness.",
                    Evidence = modernEvidence
                };
            }

            if (protocols.Any() || suites.Any() || namedGroups.Any())
            {
                return new PqcCheckResult
                {
                    Domain = domain,
                    PqcDetected = false,
                    Status = "INFO",
                    Mode = supportsTls13 ? "Classical TLS only" : "Legacy / classical TLS",
                    ReadinessLevel = "Not supported",
                    AlgorithmFamily = "Classical TLS",
                    HandshakeSupported = supportsTls13,
                    Confidence = "MEDIUM",
                    Notes = "TLS behavior was observable, but no hybrid or post-quantum indicators were detected.",
                    Evidence = new List<string>()
                };
            }

            return new PqcCheckResult
            {
                Domain = domain,
                PqcDetected = false,
                Status = "UNKNOWN",
                Mode = "Unclear",
                ReadinessLevel = "Unknown / not verifiable",
                AlgorithmFamily = "Unknown",
                HandshakeSupported = false,
                Confidence = "LOW",
                Notes = "PQC support could not be reliably determined from the available TLS analysis data."
            };
        }

        private static string DetermineAlgorithmFamily(IEnumerable<string> evidence)
        {
            var values = evidence.ToList();
            if (values.Any(value => value.Contains("MLKEM", StringComparison.OrdinalIgnoreCase) || value.Contains("KYBER", StringComparison.OrdinalIgnoreCase)))
            {
                return "ML-KEM / Kyber hybrid";
            }

            if (values.Any(value => value.Contains("HQC", StringComparison.OrdinalIgnoreCase)))
            {
                return "HQC hybrid";
            }

            if (values.Any(value => value.Contains("BIKE", StringComparison.OrdinalIgnoreCase)))
            {
                return "BIKE hybrid";
            }

            return "Post-quantum / hybrid indicators";
        }

        private static string NormalizeDomain(string domain)
        {
            return DomainInputSanitizer.NormalizeDomain(domain);
        }
    }
}
