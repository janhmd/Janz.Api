using Domain.Enums;
using Service.Helpers.DTOs.Notifications;

namespace Service.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationListDto> GetAllAsync(string userId);
        Task MarkReadAsync(string userId, int id);
        Task MarkAllReadAsync(string userId);

        // Başqa servislər (məsələn sifariş) istifadəçiyə bildiriş yazır
        Task AddAsync(string userId, NotificationKind kind, string title, string text, string? link);
    }
}
