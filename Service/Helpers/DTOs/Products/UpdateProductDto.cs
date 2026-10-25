using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Products
{
    // Admin: məhsulu dəyişmək. Şəkillər istəyə bağlıdır: göndərilməsə köhnələri qalır
    public class UpdateProductDto : IProductDetails
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();

        // Endirim, nişan və görünmə
        public decimal? OldPrice { get; set; }
        public string? Badge { get; set; }
        public bool IsVisible { get; set; } = true;

        // Məhsul səhifəsindəki bloklar (hamısı istəyə bağlıdır)
        public string? Roast { get; set; }
        public string? Region { get; set; }
        public string? Processing { get; set; }
        public string? Caffeine { get; set; }
        public string? Brewing { get; set; }
        public string? Storage { get; set; }
        public List<string> BrewMethods { get; set; } = new List<string>();

        // Kartdakı dad təsviri və filtrlər
        public string? Use { get; set; }
        public string? Taste { get; set; }
        public string? Notes { get; set; }
        public int? Density { get; set; }
        public int? Acidity { get; set; }
        public string? TasteGroup { get; set; }

        // Variantlar (istəyə bağlı). Varsa Price və Stock onlardan hesablanır
        public List<ProductVariantFormDto> Variants { get; set; } = new List<ProductVariantFormDto>();
        public bool HasVariants => Variants.Count > 0;

        // Yeni əsas şəkil: göndərilsə köhnəsini əvəz edir
        public IFormFile? Image { get; set; }

        // Qalereyaya əlavə olunacaq şəkillər
        public List<IFormFile> Gallery { get; set; } = new List<IFormFile>();

        // Qalereyadan silinəcək şəkillərin yolları (ProductDto.Images-dən götürülür)
        public List<string> RemoveImages { get; set; } = new List<string>();
    }
}
