using Service.Helpers.DTOs.Settings;

namespace Service.Services.Interfaces
{
    public interface ISettingService
    {
        Task<SiteSettingDto> GetAsync();

        // Admin panel
        Task UpdateAsync(UpdateSiteSettingDto model);
    }
}
