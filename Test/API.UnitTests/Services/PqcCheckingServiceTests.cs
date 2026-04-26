using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class PqcCheckingServiceTests
{
    [Fact]
    public async Task CheckPqcAsync_WithExplicitHybridEvidence_ReturnsDetected()
    {
        var response = new SslLabsResponse
        {
            Host = "example.com",
            Status = "READY",
            Endpoints =
            [
                new SslLabsEndpoint
                {
                    Details = new SslLabsEndpointDetails
                    {
                        Protocols =
                        [
                            new SslLabsProtocol { Name = "TLS", Version = "1.3" }
                        ],
                        Suites =
                        [
                            new SslLabsProtocolSuiteGroup
                            {
                                List =
                                [
                                    new SslLabsSuite
                                    {
                                        Name = "TLS_AES_256_GCM_SHA384",
                                        CipherStrength = 256,
                                        NamedGroupName = "X25519MLKEM768"
                                    }
                                ]
                            }
                        ]
                    }
                }
            ]
        };

        var service = new PqcCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<PqcCheckingService>.Instance);

        var result = await service.CheckPqcAsync("https://example.com");

        Assert.Equal("example.com", result.Domain);
        Assert.True(result.PqcDetected);
        Assert.Equal("Hybrid PQC supported", result.ReadinessLevel);
        Assert.Contains(result.Evidence, value => value.Contains("MLKEM", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckPqcAsync_WithModernClassicalTls_ReturnsUnknownNotVerifiable()
    {
        var response = new SslLabsResponse
        {
            Host = "example.com",
            Status = "READY",
            Endpoints =
            [
                new SslLabsEndpoint
                {
                    Details = new SslLabsEndpointDetails
                    {
                        Protocols =
                        [
                            new SslLabsProtocol { Name = "TLS", Version = "1.3" }
                        ],
                        NamedGroups =
                        [
                            new SslLabsNamedGroup { Name = "X25519", Bits = 253 }
                        ]
                    }
                }
            ]
        };

        var service = new PqcCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<PqcCheckingService>.Instance);

        var result = await service.CheckPqcAsync("example.com");

        Assert.False(result.PqcDetected);
        Assert.Equal("Unknown / not verifiable", result.ReadinessLevel);
        Assert.Equal("LOW", result.Confidence);
        Assert.True(result.HandshakeSupported);
    }

    [Fact]
    public async Task CheckPqcAsync_WhenNoTlsEvidenceExists_ReturnsUnknown()
    {
        var response = new SslLabsResponse
        {
            Host = "example.com",
            Status = "ERROR"
        };

        var service = new PqcCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<PqcCheckingService>.Instance);

        var result = await service.CheckPqcAsync("example.com");

        Assert.False(result.PqcDetected);
        Assert.Equal("UNKNOWN", result.Status);
        Assert.Equal("Unknown / not verifiable", result.ReadinessLevel);
    }

    [Fact]
    public async Task CheckPqcAsync_WhenSslLabsThrows_ReturnsUnknownFallback()
    {
        var service = new PqcCheckingService(
            new FakeSslLabsClient(new InvalidOperationException("boom")),
            NullLogger<PqcCheckingService>.Instance);

        var result = await service.CheckPqcAsync("example.com");

        Assert.Equal("UNKNOWN", result.Status);
        Assert.Contains("could not be reliably determined", result.Notes, StringComparison.OrdinalIgnoreCase);
    }
}
