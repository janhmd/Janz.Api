using Domain.Entities;
using Repository.Results;

namespace Repository.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        // Yalnız həmin istifadəçinin sifarişləri (məhsul sətirləri ilə): GetAllAsync() hamının sifarişini qaytarır
        Task<IReadOnlyList<Order>> GetByUserAsync(string userId);

        // Sifariş yalnız sahibinə aiddirsə tapılır (başqasının sifarişinə baxmağın qarşısını alır)
        Task<Order?> GetByIdForUserAsync(int id, string userId);

        // Bir tranzaksiyada: stok azalır, sifariş və sətirləri yazılır, səbət boşalır. Ya hamısı olur, ya heç biri.
        Task<OrderPlaceResult> PlaceAsync(OrderPlan plan);
    }
}
