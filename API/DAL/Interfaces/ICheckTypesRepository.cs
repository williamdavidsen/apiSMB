using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface ICheckTypeRepository
    {
        Task<CheckTypeDto> GetByIdAsync(int id);
        Task<IEnumerable<CheckTypeDto>> GetAllAsync();
        Task<CheckTypeDto> GetByCodeAsync(string code);
        Task<CheckTypeDto> AddAsync(CheckTypeDto checkTypeDto);
        Task<CheckTypeDto> UpdateAsync(CheckTypeDto checkTypeDto);
        Task<bool> DeleteAsync(int id);
    }
}
