using Domain.Common;

namespace Domain.Entities
{
    // Seçilmiş məhsul: bir istifadəçi bir məhsulu bir dəfə seçə bilər
    public class WishlistItem : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
