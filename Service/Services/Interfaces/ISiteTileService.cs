using Service.Helpers.DTOs.Tiles;

namespace Service.Services.Interfaces
{
    public interface ISiteTileService
    {
        // Sayt: bir səhifənin görünən kartları (sıra ilə)
        Task<IEnumerable<SiteTileDto>> GetVisibleAsync(string placement);

        // Admin
        Task<IEnumerable<SiteTileDto>> GetAllAsync();
        Task<SiteTileDto> GetByIdAsync(int id);
        Task<SiteTileDto> CreateAsync(SaveSiteTileDto model);
        Task UpdateAsync(int id, SaveSiteTileDto model);
        Task DeleteAsync(int id);
    }
}
