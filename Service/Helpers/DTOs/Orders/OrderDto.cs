using Service.Helpers.DTOs.Refunds;

namespace Service.Helpers.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string? CouponCode { get; set; }
        public string? LevelName { get; set; }
        public decimal Discount { get; set; }
        public decimal BalanceUsed { get; set; }

        // Çatdırılma haqqı (Total-a daxildir)
        public decimal DeliveryFee { get; set; }

        // Yalnız sifariş yaranan an: kartla ödəniləcəksə Stripe səhifəsinin ünvanı. Doludursa müştəri ora yönləndirilir
        public string? PaymentUrl { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeliveryFullName { get; set; } = string.Empty;
        public string DeliveryPhone { get; set; } = string.Empty;
        public string DeliveryLine { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;
        // Pulun geri qaytarılması: ən son sorğu (yoxdursa null) və yeni sorğu göndərmək olarmı
        public RefundDto? Refund { get; set; }
        public bool CanRequestRefund { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
