using Domain.Entities;

namespace Repository.Repositories.Interfaces
{
    public interface IBalanceRepository : IBaseRepository<BalanceTransaction>
    {
        // Yalnız təsdiqlənmiş hərəkətlərin cəmi
        Task<decimal> GetBalanceAsync(string userId);

        // Tarixçə: təsdiqlənmiş hərəkətlər, yenisi əvvəldə
        Task<IReadOnlyList<BalanceTransaction>> GetByUserAsync(string userId);

        // Kartla artırma: Stripe sessiyasına görə (dəyişdirmək üçün izlənir)
        Task<BalanceTransaction?> GetBySessionAsync(string userId, string sessionId);

        // Sertifikat bir dəfə istifadə olunur: kod "boşdursa" tutulur və eyni anda balansa yazılır.
        // Eyni kodu iki nəfər eyni anda daxil etsə yalnız biri alır. Alınmasa null
        Task<BalanceTransaction?> RedeemCertificateAsync(string userId, string code);
    }
}
