using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface ICustomerRepository
    {
        Task<CustomerDto> GetByIdAsync(int id);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> AddAsync(CustomerDto customerDto);
        Task<CustomerDto> UpdateAsync(CustomerDto customerDto);
        Task<bool> DeleteAsync(int id);
    }
}
