using Domain.Common;

namespace Domain.Entities
{
    // Variantın şəkilləri: birincisi variant seçiləndə əsas şəkil olur
    public class ProductVariantImage : BaseEntity
    {
        public int VariantId { get; set; }
        public ProductVariant Variant { get; set; } = null!;

        public string ImagePath { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
