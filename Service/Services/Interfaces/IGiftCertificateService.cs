using Service.Helpers.DTOs.GiftCertificates;

namespace Service.Services.Interfaces
{
    public interface IGiftCertificateService
    {
        // Admin panel: status = Active / Used / Expired / Disabled / boş (hamısı)
        Task<IEnumerable<GiftCertificateDto>> GetAllAsync(string? status);
        Task<GiftCertificateDto> GetByIdAsync(int id);
        Task<GiftCertificateDto> CreateAsync(CreateGiftCertificateDto model);
        Task UpdateAsync(int id, UpdateGiftCertificateDto model);
        Task DeleteAsync(int id);
    }
}
