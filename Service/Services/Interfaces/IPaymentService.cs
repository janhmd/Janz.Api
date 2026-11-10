using Service.Helpers.DTOs.Balance;
using Service.Helpers.DTOs.Payments;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IPaymentService
    {
        // Sifariş üçün Stripe Checkout sessiyası yaradır
        Task<ServiceResult<PaymentSessionDto>> CreateSessionAsync(string userId, int orderId);

        // Sessiya ödənibsə sifariş ödənmiş olur
        Task<ServiceResult<PaymentResultDto>> ConfirmAsync(string userId, int orderId, ConfirmPaymentDto model);

        // Ödəniş başladılıb, amma sifariş hələ ödənmiş sayılmırsa: Stripe-dan yenidən yoxlanılır (səhifəyə qayıdılmayıb ola bilər)
        Task RefreshAsync(string userId, int orderId);

        // Müştəri Stripe səhifəsindən geri qayıtdı və ya ödəniş alınmadı: sifariş ödənilibsə həqiqi olur, yoxsa silinir (səbət qalır)
        Task<ServiceResult<PaymentResultDto>> AbortAsync(string userId, int orderId);

        // Yeni ödəniş başlamazdan əvvəl: bu müştərinin əvvəlki yarımçıq ödəniş sifarişlərini həll edir (ödənibsə həqiqi, yoxsa silinir)
        Task SettleAwaitingAsync(string userId);

        // Fon təmizlik işi: 35 dəqiqədən köhnə gözləyən sifarişləri həll edir (müştəri Stripe-da ödəyib amma qayıtmayıbsa sifariş itmir)
        Task<int> SettleStaleAsync();

        // Balansı kartla artırmaq: təsdiqlənməmiş hərəkət və Stripe sessiyası yaradılır
        Task<ServiceResult<PaymentSessionDto>> CreateTopUpSessionAsync(string userId, TopUpDto model);

        // Sessiya ödənibsə hərəkət təsdiqlənir və məbləğ balansa düşür (bir dəfə)
        Task<ServiceResult<TopUpResultDto>> ConfirmTopUpAsync(string userId, ConfirmTopUpDto model);
    }
}
