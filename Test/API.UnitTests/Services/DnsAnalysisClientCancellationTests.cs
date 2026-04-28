using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class DnsAnalysisClientCancellationTests
{
    [Fact]
    public async Task QueryAsync_WhenCancellationIsRequested_ReturnsFailedLookupResult()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler((_, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(HttpResponseFactory.Empty(HttpStatusCode.OK));
        }));

        var client = new DnsAnalysisClient(httpClient, NullLogger<DnsAnalysisClient>.Instance);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await client.QueryAsync("example.com", "TXT", cancellation.Token);

        Assert.False(result.Succeeded);
        Assert.Contains("canceled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
