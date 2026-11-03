using Domain.Entities;

namespace Repository.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        // Ən son 100 bildiriş, yenisi əvvəldə
        Task<IReadOnlyList<Notification>> GetByUserAsync(string userId);

        Task<int> CountUnreadAsync(string userId);

        // Yalnız bu istifadəçinin bildirişi oxunmuş olur. Tapılmasa false
        Task<bool> MarkReadAsync(string userId, int id);

        Task MarkAllReadAsync(string userId);
    }
}
