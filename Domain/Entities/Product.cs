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
        public string? Brewing { get; set; }                       // qısa resept (istəyə bağlı)
        public string? BrewMethods { get; set; }                   // hazırlanma üsulları, vergüllə (Domain/Constants/BrewMethods)
        public string? Storage { get; set; }

        // Kartdakı qısa dad təsviri: "For espresso", "Dense coffee with notes of nuts, chocolate and caramel"
        public string? Use { get; set; }                           // təyinat, kartın yuxarısında
        public string? Taste { get; set; }                         // cümlənin əvvəli, məs. "Dense coffee"
        public string? Notes { get; set; }                         // notlar, vergüllə
        public int? Density { get; set; }                          // 0-100, kartdakı zolaq və filtr
        public int? Acidity { get; set; }                          // 0-100
        public string? TasteGroup { get; set; }                    // Domain/Constants/TasteGroups

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

        // Variantlar varsa Price, OldPrice və Stock onlardan hesablanır (ən ucuz variant, stokların cəmi)
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
