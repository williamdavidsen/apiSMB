using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface IFindingRepository
    {
        Task<FindingsDto?> GetByIdAsync(int id);
        Task<IEnumerable<FindingsDto>> GetAllAsync();
        Task<IEnumerable<FindingsDto>> GetByCheckResultIdAsync(int checkResultId);
        Task<FindingsDto> AddAsync(FindingsDto findingDto);
        Task<FindingsDto> UpdateAsync(FindingsDto findingDto);
        Task<bool> DeleteAsync(int id);
    }
}
