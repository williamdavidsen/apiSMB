using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class SslCheckingServiceBoundaryTests
{
    [Fact]
    public async Task GetSslDetailsAsync_WhenSslLabsNeedsRetry_PollsUntilReady()
    {
        var now = DateTimeOffset.UtcNow;
        var client = new SequencedSslLabsClient(
            new SslLabsResponse { Host = "example.com", Status = "IN_PROGRESS" },
            new SslLabsResponse
            {
                Host = "example.com",
                Status = "READY",
                Certs =
                [
                    new SslLabsCert
                    {
                        Subject = "CN=example.com",
                        IssuerSubject = "CN=Example CA",
                        NotBefore = now.AddDays(-30).ToUnixTimeMilliseconds(),
                        NotAfter = now.AddDays(90).ToUnixTimeMilliseconds()
                    }
                ],
                Endpoints =
                [
                    new SslLabsEndpoint
                    {
                        IpAddress = "203.0.113.20",
                        ServerName = "example.com",
                        Grade = "A",
                        Details = new SslLabsEndpointDetails
                        {
                            Protocols = [ new SslLabsProtocol { Name = "TLS", Version = "1.3" } ],
                            Suites =
                            [
                                new SslLabsProtocolSuiteGroup
                                {
                                    Protocol = "TLS 1.3",
                                    List = [ new SslLabsSuite { Name = "TLS_AES_256_GCM_SHA384", CipherStrength = 256 } ]
                                }
                            ]
                        }
                    }
                ]
            });

        var service = new SslCheckingService(client, NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("example.com");

        Assert.Equal("PASS", result.Status);
        Assert.Equal(2, client.CallCount);
    }

    [Fact]
    public async Task GetSslDetailsAsync_WhenCertificateExpiresWithinSevenDays_RaisesCriticalAlarm()
    {
        var now = DateTimeOffset.UtcNow;
        var service = new SslCheckingService(
            new FakeSslLabsClient(new SslLabsResponse
            {
                Host = "example.com",
                Status = "READY",
                Certs =
                [
                    new SslLabsCert
                    {
                        Subject = "CN=example.com",
                        IssuerSubject = "CN=Example CA",
                        NotBefore = now.AddDays(-20).ToUnixTimeMilliseconds(),
                        NotAfter = now.AddDays(3).ToUnixTimeMilliseconds(),
                        CommonNames = ["example.com"]
                    }
                ],
                Endpoints =
                [
                    new SslLabsEndpoint
                    {
                        IpAddress = "203.0.113.30",
                        ServerName = "example.com",
                        Grade = "B",
                        Details = new SslLabsEndpointDetails
                        {
                            Protocols = [ new SslLabsProtocol { Name = "TLS", Version = "1.2" } ],
                            Suites =
                            [
                                new SslLabsProtocolSuiteGroup
                                {
                                    Protocol = "TLS 1.2",
                                    List = [ new SslLabsSuite { Name = "TLS_AES_128_GCM_SHA256", CipherStrength = 128 } ]
                                }
                            ]
                        }
                    }
                ]
            }),
            NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("example.com");

        Assert.Contains(result.Alerts, alert => alert.Type == "CRITICAL_ALARM" && alert.Message.Contains("expire very soon", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSslDetailsAsync_WhenShortLivedCertificateHasHealthyRemainingPercentage_UsesShortLivedNarrative()
    {
        var now = DateTimeOffset.UtcNow;
        var service = new SslCheckingService(
            new FakeSslLabsClient(new SslLabsResponse
            {
                Host = "example.com",
                Status = "READY",
                Certs =
                [
                    new SslLabsCert
                    {
                        Subject = "CN=example.com",
                        IssuerSubject = "CN=Example CA",
                        NotBefore = now.AddDays(-2).ToUnixTimeMilliseconds(),
                        NotAfter = now.AddDays(4).ToUnixTimeMilliseconds()
                    }
                ],
                Endpoints =
                [
                    new SslLabsEndpoint
                    {
                        IpAddress = "203.0.113.40",
                        ServerName = "example.com",
                        Grade = "A",
                        Details = new SslLabsEndpointDetails
                        {
                            Protocols = [ new SslLabsProtocol { Name = "TLS", Version = "1.3" } ],
                            Suites =
                            [
                                new SslLabsProtocolSuiteGroup
                                {
                                    Protocol = "TLS 1.3",
                                    List = [ new SslLabsSuite { Name = "TLS_CHACHA20_POLY1305_SHA256", CipherStrength = 256 } ]
                                }
                            ]
                        }
                    }
                ]
            }),
            NullLogger<SslCheckingService>.Instance);

        var result = await service.GetSslDetailsAsync("example.com");

        Assert.Contains("Short-lived certificate", result.Criteria.RemainingLifetime.Details, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.Criteria.RemainingLifetime.Score >= 4);
    }

    [Fact]
    public async Task GetSslDetailsAsync_WhenCancellationOccursBeforeSslLabsCompletes_ReturnsErrorSummary()
    {
        var service = new SslCheckingService(
            new ThrowingSslLabsClient(new OperationCanceledException("SSL Labs polling canceled.")),
            NullLogger<SslCheckingService>.Instance);

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await service.GetSslDetailsAsync("example.invalid", cancellation.Token);

        Assert.Equal("ERROR", result.Status);
        Assert.Equal("ERROR", result.DataSource);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("did not return a usable result", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class SequencedSslLabsClient : ISslLabsClient
    {
        private readonly Queue<SslLabsResponse> _responses;

        public SequencedSslLabsClient(params SslLabsResponse[] responses)
        {
            _responses = new Queue<SslLabsResponse>(responses);
        }

        public int CallCount { get; private set; }

        public Task<SslLabsResponse> AnalyzeAsync(string domain, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed class ThrowingSslLabsClient : ISslLabsClient
    {
        private readonly Exception _exception;

        public ThrowingSslLabsClient(Exception exception)
        {
            _exception = exception;
        }

        public Task<SslLabsResponse> AnalyzeAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromException<SslLabsResponse>(_exception);
        }
    }
}
