using Service.Helpers.DTOs.Tags;

namespace Service.Helpers.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public decimal? OldPrice { get; set; }
        public string? Badge { get; set; }
        public bool IsVisible { get; set; }
        public string? Roast { get; set; }
        public string? Region { get; set; }
        public string? Processing { get; set; }
        public string? Caffeine { get; set; }
        public string? Brewing { get; set; }
        public string? Storage { get; set; }
        public List<string> BrewMethods { get; set; } = new List<string>();
        public string? Use { get; set; }
        public string? Taste { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
        public int? Density { get; set; }
        public int? Acidity { get; set; }
        public string? TasteGroup { get; set; }

        // Görünən rəylərdən: kartda ⭐ 4.6 · 687
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
    }
}
