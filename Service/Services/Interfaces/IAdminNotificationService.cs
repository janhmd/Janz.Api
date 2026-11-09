using Service.Helpers.DTOs.Dashboard;

namespace Service.Services.Interfaces
{
    public interface IAdminNotificationService
    {
        // types: işçinin rolunun görə biləcəyi bildiriş növləri (order, report, review, stock)
        Task<List<AdminNotificationDto>> GetAsync(IReadOnlyCollection<string> types);
    }
}
