using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.DAL.Interfaces
{
    public interface IAssetRepository
    {
        Task<AssetDto?> GetByIdAsync(int id);
        Task<IEnumerable<AssetDto>> GetAllAsync();
        Task<AssetDto> AddAsync(AssetDto assetDto);
        Task<AssetDto> UpdateAsync(AssetDto assetDto);
        Task<bool> DeleteAsync(int id);
    }
}
