using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class ControllerErrorPathsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ControllerErrorPathsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAssessmentCheck_WithMissingDomain_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/assessment/check", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostHeadersCheck_WithMissingDomain_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/headers/check", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostEmailCheck_WithMissingDomain_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/email/check", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostReputationCheck_WithMissingDomain_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/reputation/check", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostSslCheck_WithMissingDomain_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/ssl/check", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPqcCheck_WithBlankDomain_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/pqc/check/%20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAssessmentCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/assessment/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetHeadersCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/headers/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetEmailCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/email/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetReputationCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/reputation/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetSslCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/ssl/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetSslDetails_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/ssl/details/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetPqcCheck_WhenServiceThrows_ReturnsServerError()
    {
        var response = await _client.GetAsync($"/api/pqc/check/{CustomWebApplicationFactory.ThrowDomain}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
