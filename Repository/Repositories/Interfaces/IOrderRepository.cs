using Domain.Entities;
using Domain.Enums;
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

        // Admin: bütün sifarişlər (sətirləri və müştərisi ilə), istəsən yalnız bir statusda olanlar
        Task<IReadOnlyList<Order>> GetAllForAdminAsync(OrderStatus? status);

        // Admin: istənilən sifariş (sətirləri və müştərisi ilə)
        Task<Order?> GetByIdForAdminAsync(int id);

        // Statusu dəyişir, yalnız sifariş hələ "from" statusundadırsa (eyni anda iki işçi dəyişsə ikincisi rədd olunur).
        // Ləğv olunanda məhsullar stoka qaytarılır. Hamısı bir tranzaksiyadadır
        Task<bool> ChangeStatusAsync(int id, OrderStatus from, OrderStatus to);
    }
}
