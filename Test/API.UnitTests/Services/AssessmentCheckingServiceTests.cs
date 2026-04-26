using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class AssessmentCheckingServiceTests
{
    [Fact]
    public async Task CheckAssessmentAsync_WhenEmailHasNoMailService_RebalancesWeights()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 30, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = false, OverallScore = 0, MaxScore = 20, Status = "INFO" },
            reputation: new ReputationCheckResult { OverallScore = 20, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync("https://example.com");

        Assert.False(result.EmailModuleIncluded);
        Assert.Equal(50m, result.Weights.SslTls);
        Assert.Equal(35m, result.Weights.HttpHeaders);
        Assert.Equal(0m, result.Weights.EmailSecurity);
        Assert.Equal(15m, result.Weights.Reputation);
        Assert.Equal(100, result.OverallScore);
        Assert.Equal("A", result.Grade);
    }

    [Fact]
    public async Task CheckAssessmentAsync_WhenSslHasZeroScoreFail_FinalStatusIsFail()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 0, MaxScore = 30, Status = "FAIL" },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = 20, MaxScore = 20, Status = "PASS" },
            reputation: new ReputationCheckResult { OverallScore = 20, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.Equal("FAIL", result.Status);
        Assert.True(result.EmailModuleIncluded);
    }

    [Fact]
    public async Task CheckAssessmentAsync_WhenEmailDnsFails_UsesPartialWarningInsteadOfNoMxMessage()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 30, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = false, OverallScore = 0, MaxScore = 20, Status = "ERROR" },
            reputation: new ReputationCheckResult { OverallScore = 20, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.Equal("PARTIAL", result.Status);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("could not be completed reliably", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Alerts, alert => alert.Message.Contains("No MX record", StringComparison.OrdinalIgnoreCase));
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
