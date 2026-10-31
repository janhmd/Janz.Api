namespace Service.Helpers.DTOs.Payments
{
    // Stripe-ın ödəniş səhifəsi: MVC müştərini bu ünvana yönləndirir
    public class PaymentSessionDto
    {
        public string Url { get; set; } = string.Empty;
    }
}
