using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class HeadersCheckingServiceHttpClientTests
{
    [Fact]
    public async Task CheckHeadersAsync_WithRealHttpClients_CombinesProbeAndObservatoryData()
    {
        var observatoryHttp = new HttpClient(new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(
                HttpStatusCode.OK,
                """
                {
                  "id": 42,
                  "details_url": "https://observatory.example/report",
                  "grade": "B",
                  "score": 75,
                  "tests_failed": 1,
                  "tests_passed": 9,
                  "tests_quantity": 10
                }
                """,
                request.RequestUri))));

        var probeHttp = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, request.RequestUri),
                Content = new StringContent(string.Empty)
            };

            response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            response.Headers.Add("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'");
            response.Headers.Add("X-Frame-Options", "DENY");
            response.Headers.Add("X-Content-Type-Options", "nosniff");
            response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            return Task.FromResult(response);
        }));

        var service = new HeadersCheckingService(
            new MozillaObservatoryClient(observatoryHttp, NullLogger<MozillaObservatoryClient>.Instance),
            new HttpHeadersProbeClient(probeHttp, NullLogger<HttpHeadersProbeClient>.Instance),
            NullLogger<HeadersCheckingService>.Instance);

        var result = await service.CheckHeadersAsync("https://example.com/login");

        Assert.Equal("example.com", result.Domain);
        Assert.Equal("PASS", result.Status);
        Assert.Equal(10, result.OverallScore);
        Assert.Equal("B", result.Observatory.Grade);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("1 failed test", StringComparison.OrdinalIgnoreCase));
    }
}
