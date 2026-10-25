using Domain.Common;

namespace Domain.Entities
{
    // Məhsula rəy: yalnız məhsulu alan (sifarişi çatdırılmış) istifadəçi yaza bilər, hər məhsula bir rəy
    public class ProductReview : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int Rating { get; set; }                  // 1-5 ulduz
        public string? Text { get; set; }
        public bool IsHidden { get; set; }               // admin gizlədə bilər
        public DateTime? UpdatedAt { get; set; }
    }
}
