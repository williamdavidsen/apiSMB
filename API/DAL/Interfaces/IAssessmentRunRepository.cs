using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface IAssessmentRunRepository
    {
        Task<AssessmentRunDto> GetByIdAsync(int id);
        Task<IEnumerable<AssessmentRunDto>> GetAllAsync();
        Task<IEnumerable<AssessmentRunDto>> GetByAssetIdAsync(int assetId);
        Task<AssessmentRunDto> AddAsync(AssessmentRunDto assessmentRunDto);
        Task<AssessmentRunDto> UpdateAsync(AssessmentRunDto assessmentRunDto);
        Task<bool> DeleteAsync(int id);
    }
}
