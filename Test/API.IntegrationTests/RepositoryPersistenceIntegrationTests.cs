using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL;
using SecurityAssessmentAPI.DAL.Repositories;
using SecurityAssessmentAPI.DTOs;
using Xunit;

namespace API.IntegrationTests;

public sealed class RepositoryPersistenceIntegrationTests
{
    [Fact]
    public async Task Repositories_PersistAndLoadAssessmentGraphAcrossAssetRunCheckResultAndFinding()
    {
        await using var context = CreateContext();
        var assetRepository = new AssetRepository(context);
        var checkTypeRepository = new CheckTypeRepository(context);
        var assessmentRunRepository = new AssessmentRunRepository(context);
        var checkResultRepository = new CheckResultRepository(context);
        var findingRepository = new FindingRepository(context);

        var asset = await assetRepository.AddAsync(new AssetDto
        {
            AssetType = "Domain",
            Value = "example.com"
        });

        var checkType = await checkTypeRepository.AddAsync(new CheckTypeDto
        {
            Code = "SSL",
            Description = "TLS / SSL module"
        });

        var run = await assessmentRunRepository.AddAsync(new AssessmentRunDto
        {
            AssetId = asset.AssetId,
            StartedAt = new DateTime(2026, 4, 27, 12, 0, 0, DateTimeKind.Utc),
            FinishedAt = new DateTime(2026, 4, 27, 12, 5, 0, DateTimeKind.Utc),
            Status = "Success",
            SummaryScore = 87,
            Grade = "B"
        });

        var checkResult = await checkResultRepository.AddAsync(new CheckResultDto
        {
            CheckTypeId = checkType.CheckTypeId,
            RunId = run.RunId,
            ScorePart = 24m,
            Status = "Ok",
            RawPayload = """{ "source": "sslLabs" }""",
            NormalizedData = """{ "status": "PASS" }"""
        });

        var finding = await findingRepository.AddAsync(new FindingsDto
        {
            CheckResultId = checkResult.CheckResultId,
            Severity = "Medium",
            Title = "Renewal watch",
            Description = "Certificate renewal should be monitored.",
            Evidence = "Expires in 21 days."
        });

        var storedRun = await assessmentRunRepository.GetByIdAsync(run.RunId);
        var runResults = await checkResultRepository.GetByRunIdAsync(run.RunId);
        var runResult = await checkResultRepository.GetByRunAndCheckTypeAsync(run.RunId, checkType.CheckTypeId);
        var findings = await findingRepository.GetByCheckResultIdAsync(checkResult.CheckResultId);
        var assetRuns = await assessmentRunRepository.GetByAssetIdAsync(asset.AssetId);

        Assert.NotNull(storedRun);
        Assert.Equal("Success", storedRun!.Status);
        Assert.Single(runResults);
        Assert.NotNull(runResult);
        Assert.Equal(checkResult.CheckResultId, runResult!.CheckResultId);
        Assert.Single(findings);
        Assert.Equal(finding.ReasonId, findings.Single().ReasonId);
        Assert.Single(assetRuns);
        Assert.Equal(asset.AssetId, assetRuns.Single().AssetId);
    }

    [Fact]
    public async Task RepositoryUpdates_AreReadableFromFreshContext()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        AssessmentRunDto run;
        await using (var seedContext = CreateContext(databaseName))
        {
            var assetRepository = new AssetRepository(seedContext);
            var assessmentRunRepository = new AssessmentRunRepository(seedContext);

            var asset = await assetRepository.AddAsync(new AssetDto
            {
                AssetType = "Domain",
                Value = "portal.example.com"
            });

            run = await assessmentRunRepository.AddAsync(new AssessmentRunDto
            {
                AssetId = asset.AssetId,
                StartedAt = new DateTime(2026, 4, 27, 13, 0, 0, DateTimeKind.Utc),
                Status = "Running",
                SummaryScore = 0,
                Grade = "F"
            });
        }

        run.Status = "Partial";
        run.SummaryScore = 62;
        run.Grade = "D";
        run.FinishedAt = new DateTime(2026, 4, 27, 13, 3, 0, DateTimeKind.Utc);

        await using (var updateContext = CreateContext(databaseName))
        {
            var updateRepository = new AssessmentRunRepository(updateContext);
            await updateRepository.UpdateAsync(run);
        }

        await using var verifyContext = CreateContext(databaseName);
        var verifyRepository = new AssessmentRunRepository(verifyContext);
        var reloaded = await verifyRepository.GetAllAsync();
        var single = Assert.Single(reloaded);

        Assert.Equal("Partial", single.Status);
        Assert.Equal(62, single.SummaryScore);
        Assert.Equal("D", single.Grade);
        Assert.NotNull(single.FinishedAt);
    }

    private static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
