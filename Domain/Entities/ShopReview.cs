using Domain.Common;

namespace Domain.Entities
{
    // Mağaza rəyi: çatdırılmış sifarişə görə yazılır (mağaza və çatdırılma ulduzları, şərh). Hər sifarişə bir rəy
    public class ShopReview : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ShopRating { get; set; }               // 1-5
        public string? ShopText { get; set; }
        public int DeliveryRating { get; set; }           // 1-5
        public string? DeliveryText { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
