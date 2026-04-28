using System.Net;
using API.UnitTests.TestSupport;
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

    [Fact]
    public async Task CheckEmailAsync_WhenNoMxRecordsExist_MarksModuleNotApplicable()
    {
        var service = new EmailCheckingService(
            new FakeDnsAnalysisClient(new Dictionary<string, DnsLookupResult>(StringComparer.OrdinalIgnoreCase)
            {
                ["example.com|MX"] = new DnsLookupResult { Succeeded = true, Records = [] }
            }),
            NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("example.com");

        Assert.Equal("NOT_APPLICABLE", result.Status);
        Assert.False(result.ModuleApplicable);
        Assert.False(result.HasMailService);
        Assert.Contains(result.Alerts, alert => alert.Message.Contains("No MX record", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("v=DMARC1; p=reject", 6, "DMARC enforcement is strong")]
    [InlineData("v=DMARC1; p=reject; pct=50", 5, "only applies to 50%")]
    [InlineData("v=DMARC1; p=quarantine", 4, "DMARC enforcement is moderate")]
    [InlineData("v=DMARC1; p=quarantine; pct=25", 3, "only applies to 25%")]
    [InlineData("v=DMARC1; p=none", 3, "enforcement is weak")]
    public async Task CheckEmailAsync_DmarcDecisionTable_AssignsExpectedScoreAndNarrative(
        string dmarcRecord,
        int expectedScore,
        string expectedDetailsFragment)
    {
        var service = new EmailCheckingService(
            CreateDnsClient(
                mxRecords: ["mail.example.com"],
                txtRecords: ["v=spf1 -all"],
                dmarcRecords: [dmarcRecord],
                dkimRecordsBySelector: new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["selector1"] = ["v=DKIM1; k=rsa; p=abc123"]
                }),
            NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("example.com");

        Assert.Equal(expectedScore, result.Criteria.DmarcEnforcement.Score);
        Assert.Contains(expectedDetailsFragment, result.Criteria.DmarcEnforcement.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("v=spf1 -all", 7, "strict fail policy")]
    [InlineData("v=spf1 ~all", 5, "soft fail policy")]
    [InlineData("v=spf1 ?all", 3, "weaker than recommended")]
    public async Task CheckEmailAsync_SpfBoundaryCases_AssignExpectedScore(string spfRecord, int expectedScore, string expectedDetailsFragment)
    {
        var service = new EmailCheckingService(
            CreateDnsClient(
                mxRecords: ["mail.example.com"],
                txtRecords: [spfRecord],
                dmarcRecords: ["v=DMARC1; p=reject"],
                dkimRecordsBySelector: new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["default"] = ["v=DKIM1; k=rsa; p=abc123"]
                }),
            NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("example.com");

        Assert.Equal(expectedScore, result.Criteria.SpfVerification.Score);
        Assert.Contains(expectedDetailsFragment, result.Criteria.SpfVerification.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckEmailAsync_WhenSpfUsesRedirect_FollowsRedirectAndScoresEffectivePolicy()
    {
        var service = new EmailCheckingService(
            CreateDnsClient(
                mxRecords: ["mail.example.com"],
                txtRecords: ["v=spf1; redirect=spf.mailhost.example"],
                dmarcRecords: ["v=DMARC1; p=reject"],
                redirectedTxtRecords: new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["spf.mailhost.example"] = ["v=spf1 -all"]
                }),
            NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("example.com");

        Assert.Equal("PASS", result.Status);
        Assert.Equal(7, result.Criteria.SpfVerification.Score);
        Assert.Equal("MEDIUM", result.Criteria.SpfVerification.Confidence);
        Assert.Contains("redirect delegation", result.Criteria.SpfVerification.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckEmailAsync_WithRealDnsClientParsing_ProducesPassForStrictPolicyBundle()
    {
        var responses = new Dictionary<string, HttpResponseMessage>(StringComparer.OrdinalIgnoreCase)
        {
            ["https://dns.google/resolve?name=example.com&type=MX"] = HttpResponseFactory.Json(
                HttpStatusCode.OK,
                """
                { "Answer": [ { "data": "10 mail.example.com." } ] }
                """),
            ["https://dns.google/resolve?name=example.com&type=TXT"] = HttpResponseFactory.Json(
                HttpStatusCode.OK,
                """
                { "Answer": [ { "data": "\"v=spf1 -all\"" } ] }
                """),
            ["https://dns.google/resolve?name=_dmarc.example.com&type=TXT"] = HttpResponseFactory.Json(
                HttpStatusCode.OK,
                """
                { "Answer": [ { "data": "\"v=DMARC1; p=reject\"" } ] }
                """),
            ["https://dns.google/resolve?name=selector1._domainkey.example.com&type=TXT"] = HttpResponseFactory.Json(
                HttpStatusCode.OK,
                """
                { "Answer": [ { "data": "\"v=DKIM1; k=rsa; p=abc123\"" } ] }
                """)
        };

        var httpClient = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            if (responses.TryGetValue(request.RequestUri!.ToString(), out var response))
            {
                return Task.FromResult(response);
            }

            return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """{ "Answer": [] }"""));
        }));

        var dnsClient = new DnsAnalysisClient(httpClient, NullLogger<DnsAnalysisClient>.Instance);
        var service = new EmailCheckingService(dnsClient, NullLogger<EmailCheckingService>.Instance);

        var result = await service.CheckEmailAsync("https://example.com/path");

        Assert.Equal("example.com", result.Domain);
        Assert.Equal("PASS", result.Status);
        Assert.Equal(20, result.OverallScore);
        Assert.Contains("mail.example.com", result.DnsSummary.MxRecords);
        Assert.Contains("selector1", result.DnsSummary.DkimSelectorsFound);
        Assert.Equal("v=spf1 -all", result.DnsSummary.SpfRecord);
        Assert.Equal("v=DMARC1; p=reject", result.DnsSummary.DmarcRecord);
    }

    private static IDnsAnalysisClient CreateDnsClient(
        IReadOnlyList<string> mxRecords,
        IReadOnlyList<string> txtRecords,
        IReadOnlyList<string> dmarcRecords,
        IReadOnlyDictionary<string, List<string>>? dkimRecordsBySelector = null,
        IReadOnlyDictionary<string, List<string>>? redirectedTxtRecords = null)
    {
        var results = new Dictionary<string, DnsLookupResult>(StringComparer.OrdinalIgnoreCase)
        {
            ["example.com|MX"] = new() { Succeeded = true, Records = mxRecords.ToList() },
            ["example.com|TXT"] = new() { Succeeded = true, Records = txtRecords.ToList() },
            ["_dmarc.example.com|TXT"] = new() { Succeeded = true, Records = dmarcRecords.ToList() }
        };

        foreach (var selector in new[] { "default", "selector1", "selector2", "google", "mail", "k1", "dkim" })
        {
            results[$"{selector}._domainkey.example.com|TXT"] = new DnsLookupResult
            {
                Succeeded = true,
                Records = dkimRecordsBySelector != null && dkimRecordsBySelector.TryGetValue(selector, out var records)
                    ? records
                    : []
            };
        }

        if (redirectedTxtRecords != null)
        {
            foreach (var pair in redirectedTxtRecords)
            {
                results[$"{pair.Key}|TXT"] = new DnsLookupResult
                {
                    Succeeded = true,
                    Records = pair.Value
                };
            }
        }

        return new FakeDnsAnalysisClient(results);
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
