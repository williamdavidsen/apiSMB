using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly ApplicationDbContext _context;

        public AssetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AssetDto> GetByIdAsync(int id)
        {
            var entity = await _context.Assets
                .Include(a => a.Customer)
                .Include(a => a.AssessmentRuns)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            return entity.ToDto();
        }

        public async Task<IEnumerable<AssetDto>> GetAllAsync()
        {
            var entities = await _context.Assets
                .Include(a => a.Customer)
                .Include(a => a.AssessmentRuns)
                .ToListAsync();

            return entities.Select(a => a.ToDto());
        }

        public async Task<IEnumerable<AssetDto>> GetByCustomerIdAsync(int customerId)
        {
            var entities = await _context.Assets
                .Include(a => a.AssessmentRuns)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            return entities.Select(a => a.ToDto());
        }

        public async Task<AssetDto> AddAsync(AssetDto assetDto)
        {
            var entity = assetDto.ToEntity();
            _context.Assets.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<AssetDto> UpdateAsync(AssetDto assetDto)
        {
            var entity = assetDto.ToEntity();
            _context.Assets.Update(entity);
            await _context.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Assets.FindAsync(id);
            if (entity == null) return false;

            _context.Assets.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
