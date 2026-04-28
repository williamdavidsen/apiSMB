using SecurityAssessmentAPI.DAL;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;
using Xunit;

namespace API.UnitTests.Mapping;

public sealed class DtoMapperTests
{
    [Fact]
    public void ToDto_MapsAssetWithAssessmentRuns()
    {
        var asset = new Asset
        {
            AssetId = 42,
            AssetType = AssetType.Domain,
            Value = "example.com",
            AssessmentRuns =
            [
                new AssessmentRun
                {
                    RunId = 7,
                    AssetId = 42,
                    Status = AssessmentStatus.Success,
                    SummaryScore = 88,
                    Grade = Grade.B
                }
            ]
        };

        var dto = asset.ToDto();
        Assert.NotNull(dto);

        Assert.Equal(42, dto.AssetId);
        Assert.Equal("Domain", dto.AssetType);
        Assert.Single(dto.AssessmentRuns);
        Assert.Equal("Success", dto.AssessmentRuns[0].Status);
        Assert.Equal("B", dto.AssessmentRuns[0].Grade);
    }

    [Fact]
    public void ToEntity_WhenDtoContainsUnknownEnums_UsesSafeDefaults()
    {
        var dto = new SecurityAssessmentAPI.DTOs.AssetDto
        {
            AssetId = 1,
            AssetType = "unexpected",
            Value = "example.com"
        };

        var entity = dto.ToEntity();
        Assert.NotNull(entity);

        Assert.Equal(AssetType.Domain, entity.AssetType);
        Assert.Equal("example.com", entity.Value);
    }

    [Fact]
    public void ToDto_WhenEntityIsNull_ReturnsNullForAllSupportedTypes()
    {
        Asset? asset = null;
        AssessmentRun? assessmentRun = null;
        CheckType? checkType = null;
        CheckResult? checkResult = null;
        Finding? finding = null;

        Assert.Null(asset.ToDto());
        Assert.Null(assessmentRun.ToDto());
        Assert.Null(checkType.ToDto());
        Assert.Null(checkResult.ToDto());
        Assert.Null(finding.ToDto());
    }

    [Fact]
    public void ToEntity_WhenDtoIsNull_ReturnsNullForAllSupportedTypes()
    {
        AssetDto? asset = null;
        AssessmentRunDto? assessmentRun = null;
        CheckTypeDto? checkType = null;
        CheckResultDto? checkResult = null;
        FindingsDto? finding = null;

        Assert.Null(asset.ToEntity());
        Assert.Null(assessmentRun.ToEntity());
        Assert.Null(checkType.ToEntity());
        Assert.Null(checkResult.ToEntity());
        Assert.Null(finding.ToEntity());
    }

    [Fact]
    public void ToEntity_WhenNestedCollectionsAreNull_UsesEmptyLists()
    {
        var asset = new AssetDto
        {
            AssetId = 11,
            AssetType = "Domain",
            Value = "example.com",
            AssessmentRuns = null!
        };

        var assessmentRun = new AssessmentRunDto
        {
            RunId = 12,
            AssetId = 11,
            Status = "Success",
            Grade = "A",
            CheckResults = null!
        };

        var checkResult = new CheckResultDto
        {
            CheckResultId = 13,
            CheckTypeId = 4,
            RunId = 12,
            Status = "Passed",
            Findings = null!
        };

        Assert.Empty(asset.ToEntity()!.AssessmentRuns);
        Assert.Empty(assessmentRun.ToEntity()!.CheckResults);
        Assert.Empty(checkResult.ToEntity()!.Findings);
    }

    [Fact]
    public void ToDto_WhenNestedCollectionsAreNull_UsesEmptyLists()
    {
        var asset = new Asset
        {
            AssetId = 21,
            AssetType = AssetType.Domain,
            Value = "example.com",
            AssessmentRuns = null!
        };

        var assessmentRun = new AssessmentRun
        {
            RunId = 22,
            AssetId = 21,
            Status = AssessmentStatus.Pending,
            Grade = Grade.C,
            CheckResults = null!
        };

        var checkResult = new CheckResult
        {
            CheckResultId = 23,
            CheckTypeId = 5,
            RunId = 22,
            Status = CheckResultStatus.NotAvailable,
            Findings = null!
        };

        Assert.Empty(asset.ToDto()!.AssessmentRuns);
        Assert.Empty(assessmentRun.ToDto()!.CheckResults);
        Assert.Empty(checkResult.ToDto()!.Findings);
    }

    [Fact]
    public void ToEntity_WhenRunEnumsAreUnknown_UsesSafeDefaults()
    {
        var dto = new AssessmentRunDto
        {
            RunId = 31,
            AssetId = 2,
            Status = "mystery",
            Grade = "legendary"
        };

        var entity = dto.ToEntity();

        Assert.NotNull(entity);
        Assert.Equal(AssessmentStatus.Pending, entity.Status);
        Assert.Equal(Grade.F, entity.Grade);
    }

    [Fact]
    public void ToEntity_WhenCheckResultStatusIsUnknown_UsesNotAvailable()
    {
        var dto = new CheckResultDto
        {
            CheckResultId = 41,
            CheckTypeId = 2,
            RunId = 3,
            Status = "broken"
        };

        var entity = dto.ToEntity();

        Assert.NotNull(entity);
        Assert.Equal(CheckResultStatus.NotAvailable, entity.Status);
    }

    [Fact]
    public void ToEntity_WhenFindingSeverityIsUnknown_UsesLow()
    {
        var dto = new FindingsDto
        {
            ReasonId = 51,
            CheckResultId = 41,
            Severity = "catastrophic",
            Title = "title"
        };

        var entity = dto.ToEntity();

        Assert.NotNull(entity);
        Assert.Equal(Severity.Low, entity.Severity);
    }
}
