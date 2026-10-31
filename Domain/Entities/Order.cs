using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }                  // endirim çıxılandan sonra

        // Promo kod: sifariş anındakı kod və endirim məbləği saxlanılır
        public string? CouponCode { get; set; }
        public decimal Discount { get; set; }

        // Çatdırılma ünvanı sifariş anındakı kimi saxlanılır: istifadəçi ünvanı sonra dəyişsə və ya silsə də sifariş dəyişmir
        public string DeliveryFullName { get; set; } = string.Empty;
        public string DeliveryPhone { get; set; } = string.Empty;
        public string DeliveryLine { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
