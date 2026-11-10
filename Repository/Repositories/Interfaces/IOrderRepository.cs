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

        // Ödəniş servisi üçün: "ödənişi gözləyən" sifarişlər də tapılır (query filter söndürülür). İzlənir, dəyişdirmək olar
        Task<Order?> GetAnyByIdAsync(int id);

        // Ödənişi gözləyən sifarişlər: istifadəçinin öz sifarişləri və/və ya müəyyən vaxtdan əvvəl yaranmışlar
        Task<IReadOnlyList<Order>> GetAwaitingAsync(string? userId, DateTime? createdBefore);

        // Kart ödənişi alındı: sifariş həqiqi olur (IsPaid), sifarişə düşən məhsullar səbətdən çıxır. Artıq həqiqidirsə false
        Task<bool> FinalizePaidAsync(int id);

        // Ödəniş alınmadı: sifariş tam silinir, stok, promo kod və balans qaytarılır, səbət olduğu kimi qalır.
        // Sifariş gözləmirsə və ya ödənibsə false (heç nə silinmir)
        Task<bool> DiscardAsync(int id);
    }
}
