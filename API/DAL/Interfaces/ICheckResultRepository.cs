using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface ICheckResultRepository
    {
        Task<CheckResultDto> GetByIdAsync(int id);
        Task<IEnumerable<CheckResultDto>> GetAllAsync();
        Task<IEnumerable<CheckResultDto>> GetByRunIdAsync(int runId);
        Task<CheckResultDto> GetByRunAndCheckTypeAsync(int runId, int checkTypeId);
        Task<CheckResultDto> AddAsync(CheckResultDto checkResultDto);
        Task<CheckResultDto> UpdateAsync(CheckResultDto checkResultDto);
        Task<bool> DeleteAsync(int id);
    }
}
