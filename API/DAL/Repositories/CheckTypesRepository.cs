using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class CheckTypeRepository : ICheckTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CheckTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CheckTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _context.CheckTypes
                .Include(ct => ct.CheckResults)
                .FirstOrDefaultAsync(ct => ct.CheckTypeId == id);
            return entity.ToDto();
        }

        public async Task<IEnumerable<CheckTypeDto>> GetAllAsync()
        {
            var entities = await _context.CheckTypes
                .Include(ct => ct.CheckResults)
                .ToListAsync();
            return entities.Select(ct => ct.ToDto()!);
        }

        public async Task<CheckTypeDto?> GetByCodeAsync(string code)
        {
            var entity = await _context.CheckTypes
                .FirstOrDefaultAsync(ct => ct.Code == code);
            return entity.ToDto();
        }

        public async Task<CheckTypeDto> AddAsync(CheckTypeDto checkTypeDto)
        {
            var entity = checkTypeDto.ToEntity()!;
            _context.CheckTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto()!;
        }

        public async Task<CheckTypeDto> UpdateAsync(CheckTypeDto checkTypeDto)
        {
            var entity = checkTypeDto.ToEntity()!;
            _context.CheckTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto()!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CheckTypes.FindAsync(id);
            if (entity == null) return false;

            _context.CheckTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
