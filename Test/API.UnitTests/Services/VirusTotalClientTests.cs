using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Caching.Memory;
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
        var client = CreateClient(new HttpClient(new StubHttpMessageHandler((_, _) => throw new InvalidOperationException())), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("UNAVAILABLE", result.ProviderStatus);
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

        var client = CreateClient(new HttpClient(handler), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("example.com", result.Domain);
        Assert.Equal("NOT_FOUND", result.ProviderStatus);
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

        var client = CreateClient(new HttpClient(handler), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal(12, result.Reputation);
        Assert.Equal(1, result.MaliciousDetections);
        Assert.Equal(2, result.SuspiciousDetections);
        Assert.Equal(3, result.CommunityMaliciousVotes);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenProviderReturnsNonSuccess_ReturnsUnavailableReport()
    {
        var configuration = CreateConfiguration();
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.BadGateway, "{\"error\":\"upstream\"}", request.RequestUri)));

        var client = CreateClient(new HttpClient(handler), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("UNAVAILABLE", result.ProviderStatus);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenProviderThrows_ReturnsUnavailableReport()
    {
        var configuration = CreateConfiguration();
        var client = CreateClient(
            new HttpClient(new StubHttpMessageHandler((_, _) => throw new HttpRequestException("offline"))),
            configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("UNAVAILABLE", result.ProviderStatus);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenEnvironmentProvidesApiKey_UsesFallbackConfigurationSource()
    {
        Environment.SetEnvironmentVariable("VirusTotal__ApiKey", "env-key");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var handler = new StubHttpMessageHandler((request, _) =>
            {
                Assert.Equal("env-key", request.Headers.GetValues("x-apikey").Single());
                return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.NotFound, "{}", request.RequestUri));
            });

            var client = CreateClient(new HttpClient(handler), configuration);

            var result = await client.GetDomainReportAsync("example.com");

            Assert.NotNull(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("VirusTotal__ApiKey", null);
        }
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenPayloadUsesStringNumbers_ParsesFallbackBranches()
    {
        var configuration = CreateConfiguration();
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "data": {
                "id": "example.com",
                "attributes": {
                  "reputation": "5",
                  "last_analysis_date": "1714200000",
                  "last_analysis_stats": {
                    "malicious": "0",
                    "suspicious": "1",
                    "harmless": "4",
                    "undetected": "12"
                  },
                  "total_votes": {
                    "malicious": "2",
                    "harmless": "8"
                  }
                }
              }
            }
            """, request.RequestUri)));

        var client = CreateClient(new HttpClient(handler), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal(5, result.Reputation);
        Assert.Equal(1, result.SuspiciousDetections);
        Assert.Equal(2, result.CommunityMaliciousVotes);
        Assert.Equal(8, result.CommunityHarmlessVotes);
        Assert.NotNull(result.LastAnalysisDate);
    }

    [Fact]
    public async Task GetDomainReportAsync_WhenProviderRateLimits_ReturnsRateLimitedReport()
    {
        var configuration = CreateConfiguration();
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.TooManyRequests, "{\"error\":\"quota\"}", request.RequestUri)));

        var client = CreateClient(new HttpClient(handler), configuration);

        var result = await client.GetDomainReportAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("RATE_LIMITED", result.ProviderStatus);
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

    private static VirusTotalClient CreateClient(HttpClient httpClient, IConfiguration configuration)
    {
        return new VirusTotalClient(
            httpClient,
            configuration,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<VirusTotalClient>.Instance);
    }
}
