using Domain.Common;

namespace Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string ProductName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Variant sifariş anındakı kimi yazı ilə saxlanılır: variant sonra silinsə də sifarişdə görünür
        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }
        public string? VariantLabel { get; set; }
        public string? Grind { get; set; }
    }
}
