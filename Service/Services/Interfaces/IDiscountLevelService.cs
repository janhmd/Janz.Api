using Service.Helpers.DTOs.DiscountLevels;

namespace Service.Services.Interfaces
{
    public interface IDiscountLevelService
    {
        // Admin panel: səviyyələr və hər birində neçə müştəri
        Task<IEnumerable<DiscountLevelDto>> GetAllAsync();
        Task SaveAsync(SaveDiscountLevelsDto model);

        // Müştərinin səviyyəsi və növbəti səviyyəyə nə qədər qalıb
        Task<MyDiscountLevelDto> GetForUserAsync(string userId);
    }
}
