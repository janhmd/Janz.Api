using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Products
{
    // Admin: yeni məhsul (multipart/form-data ilə gəlir, çünki şəkil də var)
    public class CreateProductDto : IProductDetails
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

        // Əsas şəkil (kartda görünən). Boş olarsa FileService-in "Choose an image." mesajı qayıdır
        public IFormFile? Image { get; set; }

        // Məhsul səhifəsindəki qalereya üçün əlavə şəkillər (istəyə bağlı)
        public List<IFormFile> Gallery { get; set; } = new List<IFormFile>();
    }
}
