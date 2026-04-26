using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class EmailCheckingServiceTests
{
    [Fact]
    public async Task CheckEmailAsync_WhenMxLookupFails_ReturnsErrorInsteadOfNoMail()
    {
        var service = new EmailCheckingService(
            new FakeDnsAnalysisClient(new Dictionary<string, DnsLookupResult>(StringComparer.OrdinalIgnoreCase)
            {
                ["example.com|MX"] = new DnsLookupResult { Succeeded = false, ErrorMessage = "timeout" }
            }),
            NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("example.com");

        Assert.Equal("ERROR", result.Status);
        Assert.True(result.ModuleApplicable);
        Assert.False(result.HasMailService);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("could not be completed reliably", StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed class FakeDnsAnalysisClient : IDnsAnalysisClient
{
    private readonly IReadOnlyDictionary<string, DnsLookupResult> _results;

    public FakeDnsAnalysisClient(IReadOnlyDictionary<string, DnsLookupResult> results)
    {
        _results = results;
    }

    public Task<DnsLookupResult> QueryAsync(string name, string type, CancellationToken cancellationToken = default)
    {
        var key = $"{name}|{type}";
        if (_results.TryGetValue(key, out var result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(new DnsLookupResult { Succeeded = true });
    }
}
