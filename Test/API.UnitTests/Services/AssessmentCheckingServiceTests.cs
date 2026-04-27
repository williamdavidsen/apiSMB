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
    public async Task CheckAssessmentAsync_WhenReputationFailsToLoad_RebalancesWeightsAndMarksPartial()
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 24, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 8, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = 16, MaxScore = 20, Status = "PASS" },
            reputation: new ReputationCheckResult { OverallScore = 0, MaxScore = 20, Status = "ERROR" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.Equal("PARTIAL", result.Status);
        Assert.Equal(43.75m, result.Weights.SslTls);
        Assert.Equal(31.25m, result.Weights.HttpHeaders);
        Assert.Equal(25m, result.Weights.EmailSecurity);
        Assert.Equal(0m, result.Weights.Reputation);
        Assert.False(result.Modules.Reputation.Included);
        Assert.Equal(0m, result.Modules.Reputation.WeightedContribution);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("reputation analysis could not be completed reliably", StringComparison.OrdinalIgnoreCase));
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

    [Theory]
    [InlineData(90, "PASS", "A")]
    [InlineData(80, "PASS", "B")]
    [InlineData(79, "WARNING", "C")]
    [InlineData(60, "WARNING", "D")]
    [InlineData(50, "WARNING", "E")]
    [InlineData(49, "FAIL", "F")]
    public async Task CheckAssessmentAsync_UsesExpectedStatusAndGradeThresholds(int sslScore, string expectedStatus, string expectedGrade)
    {
        var moduleStatus = sslScore >= 80 ? "PASS" : sslScore >= 50 ? "WARNING" : "FAIL";
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = sslScore, MaxScore = 100, Status = moduleStatus },
            headers: new HeadersCheckResult { OverallScore = sslScore, MaxScore = 100, Status = moduleStatus },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = sslScore, MaxScore = 100, Status = moduleStatus },
            reputation: new ReputationCheckResult { OverallScore = sslScore, MaxScore = 100, Status = moduleStatus });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.Equal(sslScore, result.OverallScore);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedGrade, result.Grade);
    }

    [Theory]
    [InlineData("https://portal.example.com/path", "portal.example.com")]
    [InlineData("security@contoso.example", "contoso.example")]
    [InlineData("http://example.com/", "example.com")]
    public async Task CheckAssessmentAsync_NormalizesDomainInResponse(string input, string expectedDomain)
    {
        var service = CreateService(
            ssl: new SslCheckResult { OverallScore = 30, MaxScore = 30, Status = "PASS" },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = false, HasMailService = false, OverallScore = 0, MaxScore = 20, Status = "INFO" },
            reputation: new ReputationCheckResult { OverallScore = 15, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync(input);

        Assert.Equal(expectedDomain, result.Domain);
    }

    [Fact]
    public async Task CheckAssessmentAsync_WhenSslHasCriticalAlarm_AddsExecutiveCriticalAlert()
    {
        var service = CreateService(
            ssl: new SslCheckResult
            {
                OverallScore = 12,
                MaxScore = 30,
                Status = "WARNING",
                Alerts =
                [
                    new SslAlert { Type = "CRITICAL_ALARM", Message = "Certificate is revoked." }
                ]
            },
            headers: new HeadersCheckResult { OverallScore = 10, MaxScore = 10, Status = "PASS" },
            email: new EmailCheckResult { ModuleApplicable = true, HasMailService = true, OverallScore = 20, MaxScore = 20, Status = "PASS" },
            reputation: new ReputationCheckResult { OverallScore = 20, MaxScore = 20, Status = "PASS" });

        var result = await service.CheckAssessmentAsync("example.com");

        Assert.Contains(result.Alerts, alert =>
            string.Equals(alert.Type, "CRITICAL_ALARM", StringComparison.OrdinalIgnoreCase) &&
            alert.Message.Contains("Critical SSL/TLS findings", StringComparison.OrdinalIgnoreCase));
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
