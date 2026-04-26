using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class MozillaObservatoryClientTests
{
    [Fact]
    public async Task ScanAsync_WhenResponseIsValid_ParsesSummaryFields()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "id": 123,
              "details_url": "https://developer.mozilla.org/report/example.com",
              "algorithm_version": 3,
              "scanned_at": "2026-04-27T10:00:00Z",
              "grade": "B",
              "score": 75,
              "status_code": 200,
              "tests_failed": 2,
              "tests_passed": 8,
              "tests_quantity": 10
            }
            """, request.RequestUri));
        });

        var client = new MozillaObservatoryClient(new HttpClient(handler), NullLogger<MozillaObservatoryClient>.Instance);

        var result = await client.ScanAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal("B", result.Grade);
        Assert.Equal(75, result.Score);
        Assert.Equal(8, result.TestsPassed);
        Assert.Equal(10, result.TestsQuantity);
    }

    [Fact]
    public async Task ScanAsync_WhenProviderReturnsNonSuccess_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.BadGateway, "{\"error\":\"upstream\"}", request.RequestUri)));

        var client = new MozillaObservatoryClient(new HttpClient(handler), NullLogger<MozillaObservatoryClient>.Instance);

        var result = await client.ScanAsync("example.com");

        Assert.Null(result);
    }
}
