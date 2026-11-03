using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Notification>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .Take(100)
                    .ToListAsync();
        }

        public async Task<int> CountUnreadAsync(string userId)
        {
            return await _dbSet.CountAsync(m => m.UserId == userId && !m.IsRead);
        }

        public async Task<bool> MarkReadAsync(string userId, int id)
        {
            var changed = await _dbSet
                    .Where(m => m.Id == id && m.UserId == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
            return changed > 0;
        }

        public async Task MarkAllReadAsync(string userId)
        {
            await _dbSet
                    .Where(m => m.UserId == userId && !m.IsRead)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }
    }
}
