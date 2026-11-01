namespace Service.Services.Interfaces
{
    public interface IWishlistService
    {
        // Seçilmiş məhsulların Id-ləri (ən son seçilən əvvəldə)
        Task<IEnumerable<int>> GetProductIdsAsync(string userId);
        Task AddAsync(string userId, int productId);
        Task RemoveAsync(string userId, int productId);
    }
}
