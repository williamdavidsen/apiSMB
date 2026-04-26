using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class EmailControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmailControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheck_WhenDnsLookupFails_ReturnsErrorPayload()
    {
        var response = await _client.GetAsync("/api/email/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EmailCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("ERROR", body.Status);
        Assert.Contains(body.Alerts, alert => alert.Message.Contains("could not be completed reliably", StringComparison.OrdinalIgnoreCase));
    }
}
