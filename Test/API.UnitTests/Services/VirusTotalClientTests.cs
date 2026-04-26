using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class VirusTotalClientTests
{
    [Fact]
    public async Task GetDomainReportAsync_WhenApiKeyIsMissing_ReturnsNull()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var client = new VirusTotalClient(new HttpClient(new StubHttpMessageHandler((_, _) => throw new InvalidOperationException())), configuration, NullLogger<VirusTotalClient>.Instance);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenProviderReturnsNotFound_ReturnsEmptyReportWithPermalink()
    {
        var configuration = CreateConfiguration();
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal("test-key", request.Headers.GetValues("x-apikey").Single());
            return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.NotFound, "{}", request.RequestUri));
        });

        var client = new VirusTotalClient(new HttpClient(handler), configuration, NullLogger<VirusTotalClient>.Instance);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("example.com", result.Domain);
        Assert.Contains("/gui/domain/example.com", result.Permalink, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenResponseIsValid_ParsesProviderPayload()
    {
        var configuration = CreateConfiguration();
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "data": {
                "id": "example.com",
                "attributes": {
                  "reputation": 12,
                  "last_analysis_date": 1714200000,
                  "last_analysis_stats": {
                    "malicious": 1,
                    "suspicious": 2,
                    "harmless": 10,
                    "undetected": 77
                  },
                  "total_votes": {
                    "malicious": 3,
                    "harmless": 4
                  }
                }
              }
            }
            """, request.RequestUri)));

        var client = new VirusTotalClient(new HttpClient(handler), configuration, NullLogger<VirusTotalClient>.Instance);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal(12, result.Reputation);
        Assert.Equal(1, result.MaliciousDetections);
        Assert.Equal(2, result.SuspiciousDetections);
        Assert.Equal(3, result.CommunityMaliciousVotes);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VirusTotal:ApiKey"] = "test-key"
            })
            .Build();
    }
}
