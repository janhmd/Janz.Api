using Domain.Entities;

namespace Repository.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        // Yalnız həmin istifadəçinin sifarişləri (məhsul sətirləri ilə): GetAllAsync() hamının sifarişini qaytarır
        Task<IReadOnlyList<Order>> GetByUserAsync(string userId);

        // Sifariş yalnız sahibinə aiddirsə tapılır (başqasının sifarişinə baxmağın qarşısını alır)
        Task<Order?> GetByIdForUserAsync(int id, string userId);
    }
}
