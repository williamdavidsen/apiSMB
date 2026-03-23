using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            var entity = await _context.Customers
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            return entity.ToDto();
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customerEntities = await _context.Customers
                .Include(c => c.Assets)
                .ToListAsync();

            return customerEntities.Select(c => c.ToDto());
        }

        public async Task<CustomerDto> AddAsync(CustomerDto customerDto)
        {
            var entity = customerDto.ToEntity();
            _context.Customers.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<CustomerDto> UpdateAsync(CustomerDto customerDto)
        {
            var entity = customerDto.ToEntity();
            _context.Customers.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Customers.FindAsync(id);
            if (entity == null) return false;

            _context.Customers.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
