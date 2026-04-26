using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class FindingRepository : IFindingRepository
    {
        private readonly ApplicationDbContext _context;

        public FindingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FindingsDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Findings
                .Include(f => f.CheckResult)
                .FirstOrDefaultAsync(f => f.ReasonId == id);
            return entity.ToDto();
        }

        public async Task<IEnumerable<FindingsDto>> GetAllAsync()
        {
            var entities = await _context.Findings
                .Include(f => f.CheckResult)
                .ToListAsync();
            return entities.Select(f => f.ToDto()!);
        }

        public async Task<IEnumerable<FindingsDto>> GetByCheckResultIdAsync(int checkResultId)
        {
            var entities = await _context.Findings
                .Where(f => f.CheckResultId == checkResultId)
                .ToListAsync();
            return entities.Select(f => f.ToDto()!);
        }

        public async Task<FindingsDto> AddAsync(FindingsDto findingDto)
        {
            var entity = findingDto.ToEntity()!;
            _context.Findings.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto()!;
        }

        public async Task<FindingsDto> UpdateAsync(FindingsDto findingDto)
        {
            var entity = findingDto.ToEntity()!;
            _context.Findings.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto()!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Findings.FindAsync(id);
            if (entity == null) return false;

            _context.Findings.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
