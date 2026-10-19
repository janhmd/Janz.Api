using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<CartItem>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Product)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<CartItem?> GetByIdForUserAsync(int id, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        public async Task<CartItem?> GetByProductForUserAsync(int productId, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.ProductId == productId && m.UserId == userId);
        }
    }
}
