using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class ReputationControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReputationControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheck_WithValidDomain_ReturnsStubbedReputationResult()
    {
        var response = await _client.GetAsync("/api/reputation/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ReputationCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("WARNING", body.Status);
        Assert.Equal(12, body.OverallScore);
    }

    [Fact]
    public async Task PostCheck_WithValidDomain_ReturnsSuspiciousAlert()
    {
        var response = await _client.PostAsJsonAsync("/api/reputation/check", new ReputationCheckRequest
        {
            Domain = "example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ReputationCheckResult>();

        Assert.NotNull(body);
        Assert.Contains(body.Alerts, alert => alert.Message.Contains("suspicious detection", StringComparison.OrdinalIgnoreCase));
    }
}
