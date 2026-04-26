using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class HttpHeadersProbeClientTests
{
    [Fact]
    public async Task ProbeAsync_WhenResponseSucceeds_ReturnsStatusFinalUriAndHeaders()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/landing"),
                Content = new StringContent(string.Empty)
            };
            response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
            response.Content.Headers.Add("X-Content-Type-Options", "nosniff");
            return Task.FromResult(response);
        });

        var client = new HttpHeadersProbeClient(new HttpClient(handler), NullLogger<HttpHeadersProbeClient>.Instance);

        var result = await client.ProbeAsync("example.com");

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("https://www.example.com/landing", result.FinalUri.ToString());
        Assert.Equal("max-age=31536000", result.Headers["Strict-Transport-Security"]);
        Assert.Equal("nosniff", result.Headers["X-Content-Type-Options"]);
    }

    [Fact]
    public async Task ProbeAsync_WhenRequestThrows_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new TaskCanceledException("timeout"));
        var client = new HttpHeadersProbeClient(new HttpClient(handler), NullLogger<HttpHeadersProbeClient>.Instance);

        var result = await client.ProbeAsync("example.com");

        Assert.Null(result);
    }
}
