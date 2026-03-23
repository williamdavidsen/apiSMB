using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class CheckResultRepository : ICheckResultRepository
    {
        private readonly ApplicationDbContext _context;

        public CheckResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CheckResultDto> GetByIdAsync(int id)
        {
            var entity = await _context.CheckResults
                .Include(cr => cr.CheckType)
                .Include(cr => cr.AssessmentRun)
                .Include(cr => cr.Findings)
                .FirstOrDefaultAsync(cr => cr.CheckResultId == id);
            return entity.ToDto();
        }

        public async Task<IEnumerable<CheckResultDto>> GetAllAsync()
        {
            var entities = await _context.CheckResults
                .Include(cr => cr.CheckType)
                .Include(cr => cr.AssessmentRun)
                .Include(cr => cr.Findings)
                .ToListAsync();
            return entities.Select(cr => cr.ToDto());
        }

        public async Task<IEnumerable<CheckResultDto>> GetByRunIdAsync(int runId)
        {
            var entities = await _context.CheckResults
                .Include(cr => cr.CheckType)
                .Include(cr => cr.Findings)
                .Where(cr => cr.RunId == runId)
                .ToListAsync();
            return entities.Select(cr => cr.ToDto());
        }

        public async Task<CheckResultDto> GetByRunAndCheckTypeAsync(int runId, int checkTypeId)
        {
            var entity = await _context.CheckResults
                .Include(cr => cr.CheckType)
                .Include(cr => cr.Findings)
                .FirstOrDefaultAsync(cr => cr.RunId == runId && cr.CheckTypeId == checkTypeId);
            return entity.ToDto();
        }

        public async Task<CheckResultDto> AddAsync(CheckResultDto checkResultDto)
        {
            var entity = checkResultDto.ToEntity();
            _context.CheckResults.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<CheckResultDto> UpdateAsync(CheckResultDto checkResultDto)
        {
            var entity = checkResultDto.ToEntity();
            _context.CheckResults.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CheckResults.FindAsync(id);
            if (entity == null) return false;

            _context.CheckResults.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
