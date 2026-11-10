namespace Service.Helpers.DTOs.Orders
{
    // Səbət səhifəsi üçün: çatdırılma haqqı və pulsuz çatdırılma həddi (api/orders/delivery)
    public class DeliveryRuleDto
    {
        public decimal Fee { get; set; }
        public decimal FreeFrom { get; set; }
    }
}
