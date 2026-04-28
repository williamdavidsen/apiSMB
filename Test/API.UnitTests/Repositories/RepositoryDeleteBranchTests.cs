using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL;
using SecurityAssessmentAPI.DAL.Repositories;
using SecurityAssessmentAPI.Models.Entities;
using Xunit;

namespace API.UnitTests.Repositories;

public sealed class RepositoryDeleteBranchTests
{
    [Fact]
    public async Task AssetRepository_DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.Assets.Add(new Asset { AssetId = 1, AssetType = AssetType.Domain, Value = "example.com" });
        await context.SaveChangesAsync();
        var repository = new AssetRepository(context);

        var deleted = await repository.DeleteAsync(1);

        Assert.True(deleted);
        Assert.Empty(context.Assets);
    }

    [Fact]
    public async Task AssetRepository_DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var repository = new AssetRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AssessmentRunRepository_DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.Assets.Add(new Asset { AssetId = 1, AssetType = AssetType.Domain, Value = "example.com" });
        context.AssessmentRuns.Add(new AssessmentRun { RunId = 2, AssetId = 1, Status = AssessmentStatus.Success, Grade = Grade.B });
        await context.SaveChangesAsync();
        var repository = new AssessmentRunRepository(context);

        var deleted = await repository.DeleteAsync(2);

        Assert.True(deleted);
        Assert.Empty(context.AssessmentRuns);
    }

    [Fact]
    public async Task AssessmentRunRepository_DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var repository = new AssessmentRunRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    [Fact]
    public async Task CheckTypeRepository_DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.CheckTypes.Add(new CheckType { CheckTypeId = 3, Code = "SSL", Description = "SSL" });
        await context.SaveChangesAsync();
        var repository = new CheckTypeRepository(context);

        var deleted = await repository.DeleteAsync(3);

        Assert.True(deleted);
        Assert.Empty(context.CheckTypes);
    }

    [Fact]
    public async Task CheckTypeRepository_DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var repository = new CheckTypeRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    [Fact]
    public async Task CheckResultRepository_DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.Assets.Add(new Asset { AssetId = 1, AssetType = AssetType.Domain, Value = "example.com" });
        context.AssessmentRuns.Add(new AssessmentRun { RunId = 2, AssetId = 1, Status = AssessmentStatus.Success, Grade = Grade.B });
        context.CheckTypes.Add(new CheckType { CheckTypeId = 3, Code = "SSL", Description = "SSL" });
        context.CheckResults.Add(new CheckResult { CheckResultId = 4, RunId = 2, CheckTypeId = 3, Status = CheckResultStatus.OK });
        await context.SaveChangesAsync();
        var repository = new CheckResultRepository(context);

        var deleted = await repository.DeleteAsync(4);

        Assert.True(deleted);
        Assert.Empty(context.CheckResults);
    }

    [Fact]
    public async Task CheckResultRepository_DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var repository = new CheckResultRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    [Fact]
    public async Task FindingRepository_DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.Assets.Add(new Asset { AssetId = 1, AssetType = AssetType.Domain, Value = "example.com" });
        context.AssessmentRuns.Add(new AssessmentRun { RunId = 2, AssetId = 1, Status = AssessmentStatus.Success, Grade = Grade.B });
        context.CheckTypes.Add(new CheckType { CheckTypeId = 3, Code = "SSL", Description = "SSL" });
        context.CheckResults.Add(new CheckResult { CheckResultId = 4, RunId = 2, CheckTypeId = 3, Status = CheckResultStatus.OK });
        context.Findings.Add(new Finding { ReasonId = 5, CheckResultId = 4, Severity = Severity.Low, Title = "Finding" });
        await context.SaveChangesAsync();
        var repository = new FindingRepository(context);

        var deleted = await repository.DeleteAsync(5);

        Assert.True(deleted);
        Assert.Empty(context.Findings);
    }

    [Fact]
    public async Task FindingRepository_DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var repository = new FindingRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
