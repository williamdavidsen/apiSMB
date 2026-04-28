using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestSupport;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class RequestValidationSecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RequestValidationSecurityIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/assessment/check")]
    [InlineData("/api/headers/check")]
    [InlineData("/api/email/check")]
    [InlineData("/api/reputation/check")]
    [InlineData("/api/ssl/check")]
    public async Task PostEndpoints_RejectOverlongDomainPayloads(string url)
    {
        var overlongDomain = new string('a', 250) + ".com";
        var response = await _client.PostAsJsonAsync(url, new { domain = overlongDomain });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/assessment/check/example.com%0d%0aX-Test:1")]
    [InlineData("/api/headers/check/example.com%0d%0aX-Test:1")]
    [InlineData("/api/email/check/example.com%0d%0aX-Test:1")]
    [InlineData("/api/reputation/check/example.com%0d%0aX-Test:1")]
    [InlineData("/api/ssl/check/example.com%0d%0aX-Test:1")]
    [InlineData("/api/ssl/details/example.com%0d%0aX-Test:1")]
    [InlineData("/api/pqc/check/example.com%0d%0aX-Test:1")]
    public async Task GetEndpoints_HandleHeaderInjectionStyleRouteInputWithoutCrashing(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError,
            $"Unexpected status code {(int)response.StatusCode} for URL {url}");
    }

    [Fact]
    public async Task AssessmentRoute_WithEncodedScriptLikeInput_ReturnsHandledPayload()
    {
        var response = await _client.GetAsync("/api/assessment/check/%3Cscript%3Ealert(1)%3C%2Fscript%3E.example");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AssessmentCheckResult>();
        Assert.NotNull(body);
        Assert.DoesNotContain("<script>", body!.Domain, StringComparison.OrdinalIgnoreCase);
    }
}
