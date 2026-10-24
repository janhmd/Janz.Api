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
                    .Include(m => m.Variant)
                        .ThenInclude(v => v!.Images)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<CartItem?> GetByIdForUserAsync(int id, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        // Eyni məhsul, variant və üyütmə bir sətirdir: təkrar əlavə olunanda say artır
        public async Task<CartItem?> GetLineForUserAsync(int productId, int? variantId, string? grind, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.UserId == userId
                    && m.ProductId == productId
                    && m.VariantId == variantId
                    && m.Grind == grind);
        }
    }
}
