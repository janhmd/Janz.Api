namespace Service.Helpers.DTOs.Products
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Format { get; set; } = string.Empty;
        public string? Size { get; set; }
        public string Label { get; set; } = string.Empty;   // "Pack · 250 g": səbətdə və sifarişdə göstərilir
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
