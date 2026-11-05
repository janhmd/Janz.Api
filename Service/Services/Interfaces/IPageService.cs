using Service.Helpers.DTOs.Pages;

namespace Service.Services.Interfaces
{
    public interface IPageService
    {
        Task<IEnumerable<SitePageLinkDto>> GetLinksAsync();
        Task<SitePageDto> GetBySlugAsync(string slug);

        // Admin panel
        Task<IEnumerable<SitePageDto>> GetAllAsync();
        Task<SitePageDto> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateSitePageDto model);
    }
}
