using System.Collections.Generic;
using System.Linq;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL
{
    public static class DtoMapper
    {
        public static CustomerDto ToDto(this Customer entity)
        {
            if (entity == null) return null;
            return new CustomerDto
            {
                CustomerId = entity.CustomerId,
                Name = entity.Name,
                Telefon = entity.Telefon,
                Email = entity.Email,
                Assets = entity.Assets?.Select(a => a.ToDto()).ToList() ?? new List<AssetDto>()
            };
        }

        public static Customer ToEntity(this CustomerDto dto)
        {
            if (dto == null) return null;
            return new Customer
            {
                CustomerId = dto.CustomerId,
                Name = dto.Name,
                Telefon = dto.Telefon,
                Email = dto.Email,
                Assets = dto.Assets?.Select(a => a.ToEntity()).ToList() ?? new List<Asset>()
            };
        }

        public static AssetDto ToDto(this Asset entity)
        {
            if (entity == null) return null;
            return new AssetDto
            {
                AssetId = entity.AssetId,
                CustomerId = entity.CustomerId,
                AssetType = entity.AssetType.ToString(),
                Value = entity.Value,
                AssessmentRuns = entity.AssessmentRuns?.Select(ar => ar.ToDto()).ToList() ?? new List<AssessmentRunDto>()
            };
        }

        public static Asset ToEntity(this AssetDto dto)
        {
            if (dto == null) return null;
            return new Asset
            {
                AssetId = dto.AssetId,
                CustomerId = dto.CustomerId,
                AssetType = Enum.TryParse<AssetType>(dto.AssetType, true, out var type) ? type : AssetType.Domain,
                Value = dto.Value,
                AssessmentRuns = dto.AssessmentRuns?.Select(ar => ar.ToEntity()).ToList() ?? new List<AssessmentRun>()
            };
        }

        public static AssessmentRunDto ToDto(this AssessmentRun entity)
        {
            if (entity == null) return null;
            return new AssessmentRunDto
            {
                RunId = entity.RunId,
                AssetId = entity.AssetId,
                StartedAt = entity.StartedAt,
                FinishedAt = entity.FinishedAt,
                Status = entity.Status.ToString(),
                SummaryScore = entity.SummaryScore,
                Grade = entity.Grade.ToString(),
                CheckResults = entity.CheckResults?.Select(cr => cr.ToDto()).ToList() ?? new List<CheckResultDto>()
            };
        }

        public static AssessmentRun ToEntity(this AssessmentRunDto dto)
        {
            if (dto == null) return null;
            return new AssessmentRun
            {
                RunId = dto.RunId,
                AssetId = dto.AssetId,
                StartedAt = dto.StartedAt,
                FinishedAt = dto.FinishedAt,
                Status = Enum.TryParse<AssessmentStatus>(dto.Status, true, out var status) ? status : AssessmentStatus.Pending,
                SummaryScore = dto.SummaryScore,
                Grade = Enum.TryParse<Grade>(dto.Grade, true, out var grade) ? grade : Grade.F,
                CheckResults = dto.CheckResults?.Select(cr => cr.ToEntity()).ToList() ?? new List<CheckResult>()
            };
        }

        public static CheckTypeDto ToDto(this CheckType entity)
        {
            if (entity == null) return null;
            return new CheckTypeDto
            {
                CheckTypeId = entity.CheckTypeId,
                Code = entity.Code,
                Description = entity.Description
            };
        }

        public static CheckType ToEntity(this CheckTypeDto dto)
        {
            if (dto == null) return null;
            return new CheckType
            {
                CheckTypeId = dto.CheckTypeId,
                Code = dto.Code,
                Description = dto.Description
            };
        }

        public static CheckResultDto ToDto(this CheckResult entity)
        {
            if (entity == null) return null;
            return new CheckResultDto
            {
                CheckResultId = entity.CheckResultId,
                CheckTypeId = entity.CheckTypeId,
                RunId = entity.RunId,
                ScorePart = entity.ScorePart,
                Status = entity.Status.ToString(),
                RawPayload = entity.RawPayload,
                NormalizedData = entity.NormalizedData,
                CheckType = entity.CheckType?.ToDto(),
                Findings = entity.Findings?.Select(f => f.ToDto()).ToList() ?? new List<FindingsDto>()
            };
        }

        public static CheckResult ToEntity(this CheckResultDto dto)
        {
            if (dto == null) return null;
            return new CheckResult
            {
                CheckResultId = dto.CheckResultId,
                CheckTypeId = dto.CheckTypeId,
                RunId = dto.RunId,
                ScorePart = dto.ScorePart,
                Status = Enum.TryParse<CheckResultStatus>(dto.Status, true, out var status) ? status : CheckResultStatus.NotAvailable,
                RawPayload = dto.RawPayload,
                NormalizedData = dto.NormalizedData,
                Findings = dto.Findings?.Select(f => f.ToEntity()).ToList() ?? new List<Finding>()
            };
        }

        public static FindingsDto ToDto(this Finding entity)
        {
            if (entity == null) return null;
            return new FindingsDto
            {
                ReasonId = entity.ReasonId,
                CheckResultId = entity.CheckResultId,
                Severity = entity.Severity.ToString(),
                Title = entity.Title,
                Description = entity.Description,
                Evidence = entity.Evidence
            };
        }

        public static Finding ToEntity(this FindingsDto dto)
        {
            if (dto == null) return null;
            return new Finding
            {
                ReasonId = dto.ReasonId,
                CheckResultId = dto.CheckResultId,
                Severity = Enum.TryParse<Severity>(dto.Severity, true, out var severity) ? severity : Severity.Low,
                Title = dto.Title,
                Description = dto.Description,
                Evidence = dto.Evidence
            };
        }
    }
}
