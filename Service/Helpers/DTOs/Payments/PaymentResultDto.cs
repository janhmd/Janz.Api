namespace Service.Helpers.DTOs.Payments
{
    // Stripe-dan qayıdandan sonra: sifariş ödənibmi
    public class PaymentResultDto
    {
        public bool IsPaid { get; set; }
    }
}
