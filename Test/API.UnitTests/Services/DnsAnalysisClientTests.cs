using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class DnsAnalysisClientTests
{
    [Fact]
    public async Task QueryAsync_WhenMxRecordsExist_ParsesPriorityAndHostname()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Contains("type=MX", request.RequestUri!.Query, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "Answer": [
                { "data": "10 mx1.example.com." },
                { "data": "20 mx2.example.com." }
              ]
            }
            """, request.RequestUri));
        });

        var client = new DnsAnalysisClient(new HttpClient(handler), NullLogger<DnsAnalysisClient>.Instance);

        var result = await client.QueryAsync("example.com", "MX");

        Assert.True(result.Succeeded);
        Assert.Equal(["mx1.example.com", "mx2.example.com"], result.Records);
    }

    [Fact]
    public async Task QueryAsync_WhenTxtRecordsExist_RemovesQuotes()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "Answer": [
                { "data": "\"v=spf1 include:spf.example.com -all\"" }
              ]
            }
            """, request.RequestUri)));

        var client = new DnsAnalysisClient(new HttpClient(handler), NullLogger<DnsAnalysisClient>.Instance);

        var result = await client.QueryAsync("example.com", "TXT");

        Assert.True(result.Succeeded);
        Assert.Equal(["v=spf1 include:spf.example.com -all"], result.Records);
    }

    [Fact]
    public async Task QueryAsync_WhenRequestFails_ReturnsUnsuccessfulLookup()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("network down"));
        var client = new DnsAnalysisClient(new HttpClient(handler), NullLogger<DnsAnalysisClient>.Instance);

        var result = await client.QueryAsync("example.com", "TXT");

        Assert.False(result.Succeeded);
        Assert.Contains("network down", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
