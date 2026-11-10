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
        public string? LevelName { get; set; }              // endirim səviyyədən gəlibsə onun adı
        public decimal Discount { get; set; }

        // Çatdırılma haqqı: Total-a daxildir (məhsullar - endirim + çatdırılma). Müəyyən məbləğdən yuxarı pulsuzdur
        public decimal DeliveryFee { get; set; }

        // Kartla ödəniş başladılıb, amma pul hələ alınmayıb: sifariş müştəriyə və adminə görünmür (query filter), stok ayrılıb,
        // səbət toxunulmayıb. Ödəniş uğurlu olanda sifariş həqiqi olur, uğursuz/ləğv/vaxt bitəndə tam silinir
        public bool IsAwaitingPayment { get; set; }

        // Daxili hesabdan ödənən hissə. Kartla və ya çatdırılmada qalan məbləğ (Total - BalanceUsed) ödənir
        public decimal BalanceUsed { get; set; }

        // Kartla ödəniş (Stripe Checkout): sessiya yaranır, müştəri qayıdanda ödənib-ödənmədiyi Stripe-dan yoxlanılır
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentSessionId { get; set; }

        // Çatdırılma ünvanı sifariş anındakı kimi saxlanılır: istifadəçi ünvanı sonra dəyişsə və ya silsə də sifariş dəyişmir
        public string DeliveryFullName { get; set; } = string.Empty;
        public string DeliveryPhone { get; set; } = string.Empty;
        public string DeliveryLine { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
