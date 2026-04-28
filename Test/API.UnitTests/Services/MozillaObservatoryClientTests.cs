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

    [Fact]
    public async Task ScanAsync_WhenProviderReturnsEmptyBody_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Empty(HttpStatusCode.OK, request.RequestUri)));

        var client = new MozillaObservatoryClient(new HttpClient(handler), NullLogger<MozillaObservatoryClient>.Instance);

        var result = await client.ScanAsync("example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task ScanAsync_WhenProviderThrows_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("offline"));
        var client = new MozillaObservatoryClient(new HttpClient(handler), NullLogger<MozillaObservatoryClient>.Instance);

        var result = await client.ScanAsync("example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task ScanAsync_WhenFieldsUseFallbackValueKinds_ParsesSupportedValues()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "id": "456",
              "details_url": 42,
              "algorithm_version": "7",
              "scanned_at": "invalid-date",
              "error": true,
              "grade": null,
              "score": "88",
              "status_code": "201",
              "tests_failed": "1",
              "tests_passed": "9",
              "tests_quantity": "10"
            }
            """, request.RequestUri)));

        var client = new MozillaObservatoryClient(new HttpClient(handler), NullLogger<MozillaObservatoryClient>.Instance);

        var result = await client.ScanAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal(456, result.Id);
        Assert.Equal("42", result.DetailsUrl);
        Assert.Equal(7, result.AlgorithmVersion);
        Assert.Null(result.ScannedAt);
        Assert.Equal(bool.TrueString, result.Error);
        Assert.Equal("UNKNOWN", result.Grade);
        Assert.Equal(88, result.Score);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(9, result.TestsPassed);
    }
}
