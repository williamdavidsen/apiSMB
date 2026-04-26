using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class SslControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SslControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheck_WithValidDomain_ReturnsStubbedSslSummary()
    {
        var response = await _client.GetAsync("/api/ssl/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SslCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("WARNING", body.Status);
        Assert.Equal(24, body.OverallScore);
    }

    [Fact]
    public async Task GetDetails_WithValidDomain_ReturnsStubbedSslDetails()
    {
        var response = await _client.GetAsync("/api/ssl/details/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SslDetailResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("DIRECT_TLS", body.DataSource);
        Assert.Contains(body.Alerts, alert => alert.Message.Contains("direct TLS probe", StringComparison.OrdinalIgnoreCase));
    }
}
