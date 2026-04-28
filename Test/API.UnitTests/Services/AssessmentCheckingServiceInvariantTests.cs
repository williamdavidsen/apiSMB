using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class AssessmentCheckingServiceInvariantTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task CheckAssessmentAsync_WeightsForIncludedModules_AlwaysSumToOneHundred(bool emailIncluded, bool reputationIncluded)
    {
        var email = emailIncluded
            ? new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = 18, MaxScore = 20, Status = "PASS" }
            : new EmailCheckResult { ModuleApplicable = false, HasMailService = false, OverallScore = 0, MaxScore = 20, Status = "NOT_APPLICABLE" };

        var reputation = reputationIncluded
            ? new ReputationCheckResult { OverallScore = 18, MaxScore = 20, Status = "PASS" }
            : new ReputationCheckResult { OverallScore = 0, MaxScore = 20, Status = "ERROR" };

        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 27, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 8, MaxScore = 10, Status = "PASS" },
            email: email,
            reputation: reputation);

        var result = await service.CheckAssessmentAsync("example.com");

        var includedWeightSum =
            result.Modules.SslTls.WeightPercent +
            result.Modules.HttpHeaders.WeightPercent +
            result.Modules.EmailSecurity.WeightPercent +
            result.Modules.Reputation.WeightPercent;

        Assert.Equal(100m, includedWeightSum);
    }

    [Fact]
    public async Task CheckAssessmentAsync_ModuleContributionsNeverExceedConfiguredWeight()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 30, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = 20, MaxScore = 20, Status = "PASS" },
            reputation: new ReputationCheckResult { OverallScore = 20, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.InRange(result.Modules.SslTls.WeightedContribution, 0m, result.Modules.SslTls.WeightPercent);
        Assert.InRange(result.Modules.HttpHeaders.WeightedContribution, 0m, result.Modules.HttpHeaders.WeightPercent);
        Assert.InRange(result.Modules.EmailSecurity.WeightedContribution, 0m, result.Modules.EmailSecurity.WeightPercent);
        Assert.InRange(result.Modules.Reputation.WeightedContribution, 0m, result.Modules.Reputation.WeightPercent);
    }

    [Fact]
    public async Task CheckAssessmentAsync_ExcludedModulesAlwaysHaveZeroWeightAndContribution()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 21, MaxScore = 30, Status = "WARNING" },
            headers: new HeadersCheckResult { OverallScore = 5, MaxScore = 10, Status = "WARNING" },
            email: new EmailCheckResult { ModuleApplicable = false, HasMailService = false, OverallScore = 0, MaxScore = 20, Status = "NOT_APPLICABLE" },
            reputation: new ReputationCheckResult { OverallScore = 0, MaxScore = 20, Status = "ERROR" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.False(result.Modules.EmailSecurity.Included);
        Assert.Equal(0m, result.Modules.EmailSecurity.WeightPercent);
        Assert.Equal(0m, result.Modules.EmailSecurity.WeightedContribution);
        Assert.False(result.Modules.Reputation.Included);
        Assert.Equal(0m, result.Modules.Reputation.WeightPercent);
        Assert.Equal(0m, result.Modules.Reputation.WeightedContribution);
    }

    private static AssessmentCheckingService CreateService(
        SslCheckResult ssl,
        HeadersCheckResult headers,
        EmailCheckResult email,
        ReputationCheckResult reputation)
    {
        return new AssessmentCheckingService(
            new FakeSslCheckingService(ssl),
            new FakeHeadersCheckingService(headers),
            new FakeEmailCheckingService(email),
            new FakeReputationCheckingService(reputation),
            new FakePqcCheckingService(new PqcCheckResult { Status = "INFO" }),
            NullLogger<AssessmentCheckingService>.Instance);
    }
}
