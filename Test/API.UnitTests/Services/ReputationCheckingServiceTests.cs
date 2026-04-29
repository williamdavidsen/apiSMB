using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class ReputationCheckingServiceTests
{
    [Fact]
    public async Task CheckReputationAsync_WithCleanSignals_ReturnsPass()
    {
        var report = new VirusTotalDomainReport
        {
            Reputation = 12,
            MaliciousDetections = 0,
            SuspiciousDetections = 0,
            CommunityMaliciousVotes = 0,
            CommunityHarmlessVotes = 4,
            HarmlessDetections = 5,
            UndetectedDetections = 20,
            Permalink = "https://www.virustotal.com/gui/domain/example.com"
        };

        var service = new ReputationCheckingService(
            new FakeVirusTotalClient(report),
            NullLogger<ReputationCheckingService>.Instance);

        var result = await service.CheckReputationAsync("https://example.com/path");

        Assert.Equal("example.com", result.Domain);
        Assert.Equal("PASS", result.Status);
        Assert.Equal(20, result.OverallScore);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("report link", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckReputationAsync_WithMixedSignals_ReturnsWarning()
    {
        var report = new VirusTotalDomainReport
        {
            Reputation = -2,
            MaliciousDetections = 0,
            SuspiciousDetections = 2,
            CommunityMaliciousVotes = 1,
            CommunityHarmlessVotes = 0
        };

        var service = new ReputationCheckingService(
            new FakeVirusTotalClient(report),
            NullLogger<ReputationCheckingService>.Instance);

        var result = await service.CheckReputationAsync("admin@example.com");

        Assert.Equal("example.com", result.Domain);
        Assert.Equal("WARNING", result.Status);
        Assert.Equal(12, result.OverallScore);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("suspicious", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckReputationAsync_WhenMaliciousDetectionsExist_ReturnsFail()
    {
        var report = new VirusTotalDomainReport
        {
            Reputation = -20,
            MaliciousDetections = 3,
            SuspiciousDetections = 1,
            CommunityMaliciousVotes = 2
        };

        var service = new ReputationCheckingService(
            new FakeVirusTotalClient(report),
            NullLogger<ReputationCheckingService>.Instance);

        var result = await service.CheckReputationAsync("example.com");

        Assert.Equal("FAIL", result.Status);
        Assert.Equal(0, result.Criteria.BlacklistStatus.Score);
        Assert.Equal(0, result.Criteria.MalwareAssociation.Score);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("malicious detection", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckReputationAsync_WhenProviderDataIsUnavailable_ReturnsUnavailable()
    {
        var service = new ReputationCheckingService(
            new FakeVirusTotalClient(new VirusTotalDomainReport
            {
                ProviderStatus = "UNAVAILABLE",
                ProviderMessage = "VirusTotal quota or rate limit was reached."
            }),
            NullLogger<ReputationCheckingService>.Instance);

        var result = await service.CheckReputationAsync("example.com");

        Assert.Equal("UNAVAILABLE", result.Status);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("quota", StringComparison.OrdinalIgnoreCase));
    }
}
