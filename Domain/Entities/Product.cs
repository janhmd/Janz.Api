using Domain.Common;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImagePath { get; set; } = string.Empty;

        // Endirimdən əvvəlki qiymət: yalnız Price-dan böyük olanda kartda üstüxətli görünür
        public decimal? OldPrice { get; set; }

        // Kartdakı nişan (Domain/Constants/ProductBadges), boş ola bilər
        public string? Badge { get; set; }

        // Gizli məhsul saytda görünmür, admin paneldə qalır
        public bool IsVisible { get; set; } = true;

        // Məhsul səhifəsindəki "Details", "Brewing" və "Storage" blokları. Boş olanlar göstərilmir
        public string? Roast { get; set; }
        public string? Region { get; set; }
        public string? Processing { get; set; }
        public string? Caffeine { get; set; }
        public string? Brewing { get; set; }
        public string? Storage { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
