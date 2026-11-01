using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    // Daxili hesabın hərəkəti: artırma (+) və ya sifarişə silinmə (-). Balans təsdiqlənmiş hərəkətlərin cəmidir
    public class BalanceTransaction : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public decimal Amount { get; set; }
        public BalanceTransactionType Type { get; set; }
        public string? Note { get; set; }

        // Sifarişlə bağlı hərəkətdə sifarişin nömrəsi (FK yoxdur: sifariş tarixçəsi ayrıca saxlanılır)
        public int? OrderId { get; set; }

        // Kartla artırmada Stripe sessiyası: ödəniş təsdiqlənənə qədər IsConfirmed false qalır
        public string? PaymentSessionId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
