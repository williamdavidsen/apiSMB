using System.Net;
using System.Text.Json;
using API.IntegrationTests.TestSupport;
using Xunit;

namespace API.IntegrationTests;

public sealed class ApiContractIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiContractIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AssessmentEndpoint_ExposesStableDashboardContractShape()
    {
        var response = await _client.GetAsync("/api/assessment/check/example.com");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        AssertString(root, "domain");
        AssertNumber(root, "overallScore");
        AssertNumber(root, "maxScore");
        AssertString(root, "status");
        AssertString(root, "grade");
        AssertBoolean(root, "emailModuleIncluded");
        AssertProperty(root, "pqcReadiness", JsonValueKind.Object);
        AssertProperty(root, "weights", JsonValueKind.Object);
        AssertProperty(root, "modules", JsonValueKind.Object);
        AssertProperty(root, "alerts", JsonValueKind.Array);

        var weights = root.GetProperty("weights");
        AssertNumber(weights, "sslTls");
        AssertNumber(weights, "httpHeaders");
        AssertNumber(weights, "emailSecurity");
        AssertNumber(weights, "reputation");

        var modules = root.GetProperty("modules");
        AssertModuleScore(modules, "sslTls");
        AssertModuleScore(modules, "httpHeaders");
        AssertModuleScore(modules, "emailSecurity");
        AssertModuleScore(modules, "reputation");
    }

    [Fact]
    public async Task SslDetailsEndpoint_ExposesNestedEvidenceCollectionsExpectedByFrontend()
    {
        var response = await _client.GetAsync("/api/ssl/details/example.com");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        AssertString(root, "domain");
        AssertString(root, "dataSource");
        AssertString(root, "status");
        AssertProperty(root, "criteria", JsonValueKind.Object);
        AssertProperty(root, "alerts", JsonValueKind.Array);
        AssertProperty(root, "endpoints", JsonValueKind.Array);
        AssertProperty(root, "certificate", JsonValueKind.Object);
        AssertProperty(root, "supportedTlsVersions", JsonValueKind.Array);
        AssertProperty(root, "notableCipherSuites", JsonValueKind.Array);

        var endpoints = root.GetProperty("endpoints");
        if (endpoints.GetArrayLength() > 0)
        {
            var endpoint = endpoints[0];
            AssertString(endpoint, "ipAddress");
            AssertString(endpoint, "serverName");
            AssertString(endpoint, "grade");
        }
    }

    [Fact]
    public async Task EmailEndpoint_ErrorPayload_StillPreservesFrontendFields()
    {
        var response = await _client.GetAsync("/api/email/check/example.com");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        AssertString(root, "domain");
        AssertBoolean(root, "hasMailService");
        AssertBoolean(root, "moduleApplicable");
        AssertNumber(root, "overallScore");
        AssertNumber(root, "maxScore");
        AssertString(root, "status");
        AssertProperty(root, "criteria", JsonValueKind.Object);
        AssertProperty(root, "dnsSummary", JsonValueKind.Object);
        AssertProperty(root, "alerts", JsonValueKind.Array);
    }

    private static void AssertModuleScore(JsonElement parent, string propertyName)
    {
        var module = parent.GetProperty(propertyName);
        AssertBoolean(module, "included");
        AssertNumber(module, "weightPercent");
        AssertNumber(module, "rawScore");
        AssertNumber(module, "rawMaxScore");
        AssertNumber(module, "normalizedScore");
        AssertNumber(module, "weightedContribution");
        AssertString(module, "status");
    }

    private static void AssertProperty(JsonElement element, string propertyName, JsonValueKind kind)
    {
        Assert.True(element.TryGetProperty(propertyName, out var property), $"Expected property '{propertyName}'.");
        Assert.Equal(kind, property.ValueKind);
    }

    private static void AssertString(JsonElement element, string propertyName)
    {
        AssertProperty(element, propertyName, JsonValueKind.String);
    }

    private static void AssertBoolean(JsonElement element, string propertyName)
    {
        Assert.True(element.TryGetProperty(propertyName, out var property), $"Expected property '{propertyName}'.");
        Assert.True(property.ValueKind is JsonValueKind.True or JsonValueKind.False, $"Expected boolean property '{propertyName}'.");
    }

    private static void AssertNumber(JsonElement element, string propertyName)
    {
        AssertProperty(element, propertyName, JsonValueKind.Number);
    }
}
