using SecurityAssessmentAPI.DAL;
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

        Assert.Equal(AssetType.Domain, entity.AssetType);
        Assert.Equal("example.com", entity.Value);
    }
}
