using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class HeadersCheckingServiceTests
{
    [Fact]
    public async Task CheckHeadersAsync_WithStrongSecurityHeaders_ReturnsPass()
    {
        var probe = new HttpHeadersProbeResult
        {
            FinalUri = new Uri("https://example.com"),
            StatusCode = 200,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains",
                ["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'",
                ["X-Content-Type-Options"] = "nosniff",
                ["Referrer-Policy"] = "strict-origin-when-cross-origin"
            }
        };

        var service = CreateService(probe);

        var result = await service.CheckHeadersAsync("https://example.com/");

        Assert.Equal("example.com", result.Domain);
        Assert.Equal(10, result.OverallScore);
        Assert.Equal("PASS", result.Status);
        Assert.DoesNotContain(result.Alerts, alert => alert.Type == "CRITICAL_WARNING");
    }

    [Fact]
    public async Task CheckHeadersAsync_WhenCriticalHeadersAreMissing_ReturnsFailWithAlerts()
    {
        var probe = new HttpHeadersProbeResult
        {
            FinalUri = new Uri("https://example.com"),
            StatusCode = 200,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        var service = CreateService(probe);

        var result = await service.CheckHeadersAsync("example.com");

        Assert.Equal(0, result.OverallScore);
        Assert.Equal("FAIL", result.Status);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("HSTS", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("Content-Security-Policy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckHeadersAsync_WhenHttpsProbeFailsButHttpRedirectsToHttps_ReturnsRedirectInfo()
    {
        var httpProbe = new HttpHeadersProbeResult
        {
            FinalUri = new Uri("https://example.com"),
            StatusCode = 200,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Strict-Transport-Security"] = "max-age=31536000",
                ["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'"
            }
        };

        var probeClient = new FakeHttpHeadersProbeClient()
            .WithResponse(Uri.UriSchemeHttps, null)
            .WithResponse(Uri.UriSchemeHttp, httpProbe);

        var service = new HeadersCheckingService(
            new FakeMozillaObservatoryClient(),
            probeClient,
            NullLogger<HeadersCheckingService>.Instance);

        var result = await service.CheckHeadersAsync("example.com");

        Assert.Equal("PASS", result.Status);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("redirected to HTTPS", StringComparison.OrdinalIgnoreCase));
    }

    private static HeadersCheckingService CreateService(HttpHeadersProbeResult probe)
    {
        var probeClient = new FakeHttpHeadersProbeClient()
            .WithResponse(Uri.UriSchemeHttps, probe);

        return new HeadersCheckingService(
            new FakeMozillaObservatoryClient(),
            probeClient,
            NullLogger<HeadersCheckingService>.Instance);
    }
}
