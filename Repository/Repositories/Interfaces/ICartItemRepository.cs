using Domain.Entities;

namespace Repository.Repositories.Interfaces
{
    public interface ICartItemRepository : IBaseRepository<CartItem>
    {
        // Yalnız həmin istifadəçinin səbəti (məhsulu ilə): GetAllAsync() hamının səbətini qaytarır
        Task<IReadOnlyList<CartItem>> GetByUserAsync(string userId);

        // Sətir yalnız sahibinə aiddirsə tapılır (başqasının səbətini dəyişməyin qarşısını alır). Dəyişdirmək üçün izlənir.
        Task<CartItem?> GetByIdForUserAsync(int id, string userId);

        // Eyni məhsul səbətdə artıq varsa həmin sətir (yeni sətir yox, say artırılır). Dəyişdirmək üçün izlənir.
        Task<CartItem?> GetLineForUserAsync(int productId, int? variantId, string? grind, string userId);
    }
}
