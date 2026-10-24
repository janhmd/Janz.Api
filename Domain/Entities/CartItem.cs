using Domain.Common;

namespace Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // Variantı olan məhsulda hansı variant seçilib və üyütmə (paket üçün)
        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }
        public string? Grind { get; set; }
    }
}
