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
    }
}
