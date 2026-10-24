using Domain.Common;

namespace Domain.Entities
{
    // Məhsulun satış variantı: paket növü (Pack, Nespresso capsules 10 pcs) və ölçü (250 g, 1000 g).
    // Qiymət, stok və şəkillər variantın özünündür
    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Format { get; set; } = string.Empty;
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public int Order { get; set; }

        public ICollection<ProductVariantImage> Images { get; set; } = new List<ProductVariantImage>();

        // Səbətdə və sifarişdə göstərilən yazı: "Pack · 250 g". Setter yoxdur, bazaya yazılmır
        public string Label => string.IsNullOrWhiteSpace(Size) ? Format : $"{Format} · {Size}";
    }
}
