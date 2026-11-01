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

        // Balansı kartla artırmaq: təsdiqlənməmiş hərəkət və Stripe sessiyası yaradılır
        Task<ServiceResult<PaymentSessionDto>> CreateTopUpSessionAsync(string userId, TopUpDto model);

        // Sessiya ödənibsə hərəkət təsdiqlənir və məbləğ balansa düşür (bir dəfə)
        Task<ServiceResult<TopUpResultDto>> ConfirmTopUpAsync(string userId, ConfirmTopUpDto model);
    }
}
