using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class SslCheckingServiceTests
{
    [Fact]
    public async Task CheckSslAsync_WithStrongSslLabsData_ReturnsPassAndFullScore()
    {
        var now = DateTimeOffset.UtcNow;
        var response = new SslLabsResponse
        {
            Host = "example.com",
            Status = "READY",
            Certs =
            [
                new SslLabsCert
                {
                    Subject = "CN=example.com",
                    IssuerSubject = "CN=Example Issuer",
                    NotBefore = now.AddDays(-30).ToUnixTimeMilliseconds(),
                    NotAfter = now.AddDays(120).ToUnixTimeMilliseconds(),
                    Sha256Hash = "abc123",
                    SignatureAlgorithm = "sha256WithRSAEncryption",
                    KeyAlgorithm = "RSA",
                    KeySize = 2048,
                    CommonNames = ["example.com"],
                    AltNames = ["example.com", "www.example.com"]
                }
            ],
            Endpoints =
            [
                new SslLabsEndpoint
                {
                    IpAddress = "203.0.113.10",
                    ServerName = "example.com",
                    Grade = "A",
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
                                Protocol = "TLS 1.3",
                                List =
                                [
                                    new SslLabsSuite { Name = "TLS_AES_256_GCM_SHA384", CipherStrength = 256 }
                                ]
                            }
                        ]
                    }
                }
            ]
        };

        var service = new SslCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("example.com");

        Assert.Equal("SSL_LABS", result.DataSource);
        Assert.Equal("PASS", result.Status);
        Assert.Equal(30, result.OverallScore);
        Assert.Contains("TLS 1.3", result.SupportedTlsVersions);
        Assert.Contains(result.NotableCipherSuites, suite => suite.Contains("256 bits", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSslDetailsAsync_WhenCertificateIsExpired_ReturnsFail()
    {
        var now = DateTimeOffset.UtcNow;
        var response = new SslLabsResponse
        {
            Host = "expired.example",
            Status = "READY",
            Certs =
            [
                new SslLabsCert
                {
                    Subject = "CN=expired.example",
                    IssuerSubject = "CN=Example Issuer",
                    NotBefore = now.AddDays(-120).ToUnixTimeMilliseconds(),
                    NotAfter = now.AddDays(-1).ToUnixTimeMilliseconds()
                }
            ],
            Endpoints =
            [
                new SslLabsEndpoint
                {
                    IpAddress = "203.0.113.11",
                    ServerName = "expired.example",
                    Grade = "F",
                    Details = new SslLabsEndpointDetails()
                }
            ]
        };

        var service = new SslCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("expired.example");

        Assert.Equal("FAIL", result.Status);
        Assert.Equal(0, result.OverallScore);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSslDetailsAsync_WhenNoEndpointsAreReturned_ReturnsFail()
    {
        var now = DateTimeOffset.UtcNow;
        var response = new SslLabsResponse
        {
            Host = "no-endpoint.example",
            Status = "READY",
            Certs =
            [
                new SslLabsCert
                {
                    Subject = "CN=no-endpoint.example",
                    IssuerSubject = "CN=Example Issuer",
                    NotBefore = now.AddDays(-5).ToUnixTimeMilliseconds(),
                    NotAfter = now.AddDays(30).ToUnixTimeMilliseconds()
                }
            ]
        };

        var service = new SslCheckingService(
            new FakeSslLabsClient(response),
            NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("no-endpoint.example");

        Assert.Equal("FAIL", result.Status);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("No HTTPS endpoint", StringComparison.OrdinalIgnoreCase));
    }
}
