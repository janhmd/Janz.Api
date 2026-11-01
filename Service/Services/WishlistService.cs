using Domain.Entities;
using Repository.Repositories.Interfaces;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IBaseRepository<WishlistItem> _wishlistRepo;
        private readonly IBaseRepository<Product> _productRepo;

        public WishlistService(IBaseRepository<WishlistItem> wishlistRepo, IBaseRepository<Product> productRepo)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
        }

        public async Task<IEnumerable<int>> GetProductIdsAsync(string userId)
        {
            return (await _wishlistRepo.GetAllAsync())
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.ProductId)
                .ToList();
        }

        // Artıq seçilibsə heç nə dəyişmir
        public async Task AddAsync(string userId, int productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null || !product.IsVisible)
            {
                throw new NotFoundException();
            }

            var exists = (await _wishlistRepo.GetAllAsync()).Any(m => m.UserId == userId && m.ProductId == productId);
            if (!exists)
            {
                await _wishlistRepo.AddAsync(new WishlistItem { UserId = userId, ProductId = productId });
            }
        }

        public async Task RemoveAsync(string userId, int productId)
        {
            var item = (await _wishlistRepo.GetAllAsync()).FirstOrDefault(m => m.UserId == userId && m.ProductId == productId);
            if (item is not null)
            {
                await _wishlistRepo.DeleteAsync(item);
            }
        }
    }
}
