using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class HeadersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HeadersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCheck_WithValidDomain_ReturnsOkResult()
    {
        var response = await _client.PostAsJsonAsync("/api/headers/check", new HeadersCheckRequest
        {
            Domain = "example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HeadersCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
        Assert.Equal("PASS", body.Status);
    }

    [Fact]
    public async Task GetCheck_WithValidDomain_ReturnsOkResult()
    {
        var response = await _client.GetAsync("/api/headers/check/example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HeadersCheckResult>();

        Assert.NotNull(body);
        Assert.Equal("example.com", body.Domain);
    }
}
