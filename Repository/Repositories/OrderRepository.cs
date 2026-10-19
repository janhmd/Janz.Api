using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Order>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<Order?> GetByIdForUserAsync(int id, string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }
    }
}
