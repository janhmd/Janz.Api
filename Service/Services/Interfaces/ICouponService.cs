using Service.Helpers.DTOs.Coupons;

namespace Service.Services.Interfaces
{
    public interface ICouponService
    {
        // Admin panel: status = Active / Scheduled / Expired / Used up / Disabled / boş (hamısı)
        Task<IEnumerable<CouponDto>> GetAllAsync(string? status);
        Task<CouponDto> GetByIdAsync(int id);
        Task<CouponDto> CreateAsync(CreateCouponDto model);
        Task UpdateAsync(int id, UpdateCouponDto model);
        Task DeleteAsync(int id);

        // Səbət: kod bu məbləğə tətbiq oluna bilərmi, endirim nə qədərdir
        Task<CouponCheckDto> CheckAsync(string? code, decimal total);
    }
}
