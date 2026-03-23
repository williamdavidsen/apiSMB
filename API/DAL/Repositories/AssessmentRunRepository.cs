using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class AssessmentRunRepository : IAssessmentRunRepository
    {
        private readonly ApplicationDbContext _context;

        public AssessmentRunRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AssessmentRunDto> GetByIdAsync(int id)
        {
            var entity = await _context.AssessmentRuns
                .Include(ar => ar.Asset)
                .Include(ar => ar.CheckResults)
                    .ThenInclude(cr => cr.Findings)
                .FirstOrDefaultAsync(ar => ar.RunId == id);

            return entity.ToDto();
        }

        public async Task<IEnumerable<AssessmentRunDto>> GetAllAsync()
        {
            var entities = await _context.AssessmentRuns
                .Include(ar => ar.Asset)
                .Include(ar => ar.CheckResults)
                    .ThenInclude(cr => cr.Findings)
                .ToListAsync();

            return entities.Select(ar => ar.ToDto());
        }

        public async Task<IEnumerable<AssessmentRunDto>> GetByAssetIdAsync(int assetId)
        {
            var entities = await _context.AssessmentRuns
                .Include(ar => ar.CheckResults)
                    .ThenInclude(cr => cr.Findings)
                .Where(ar => ar.AssetId == assetId)
                .ToListAsync();

            return entities.Select(ar => ar.ToDto());
        }

        public async Task<AssessmentRunDto> AddAsync(AssessmentRunDto assessmentRunDto)
        {
            var entity = assessmentRunDto.ToEntity();
            _context.AssessmentRuns.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<AssessmentRunDto> UpdateAsync(AssessmentRunDto assessmentRunDto)
        {
            var entity = assessmentRunDto.ToEntity();
            _context.AssessmentRuns.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            var dbEntity = await _context.AssessmentRuns.FindAsync(id);
            if (dbEntity == null) return false;

            _context.AssessmentRuns.Remove(dbEntity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
