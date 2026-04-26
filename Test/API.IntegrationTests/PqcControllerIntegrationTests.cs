using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class PqcControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PqcControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheck_WithValidDomain_ReturnsStubbedPqcInsight()
    {
        var response = await _client.GetAsync("/api/pqc/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PqcCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("INFO", body.Status);
        Assert.Equal("Unknown / not verifiable", body.ReadinessLevel);
    }
}
