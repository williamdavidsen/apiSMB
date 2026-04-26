using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class AssessmentControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AssessmentControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheck_WhenAssessmentIsPartial_ReturnsExpectedWarningSummary()
    {
        var response = await _client.GetAsync("/api/assessment/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AssessmentCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("PARTIAL", body.Status);
        Assert.Equal("B", body.Grade);
        Assert.Contains(body.Alerts, alert => alert.Message.Contains("could not be completed reliably", StringComparison.OrdinalIgnoreCase));
    }
}
