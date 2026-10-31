namespace Service.Helpers.DTOs.Payments
{
    // Stripe müştərini geri qaytaranda ünvanda gələn sessiya (session_id)
    public class ConfirmPaymentDto
    {
        public string? SessionId { get; set; }
    }
}
