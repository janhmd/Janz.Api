using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Products
{
    // Admin: yeni məhsul (multipart/form-data ilə gəlir, çünki şəkil də var)
    public class CreateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();

        // Əsas şəkil (kartda görünən). Boş olarsa FileService-in "Choose an image." mesajı qayıdır
        public IFormFile? Image { get; set; }

        // Məhsul səhifəsindəki qalereya üçün əlavə şəkillər (istəyə bağlı)
        public List<IFormFile> Gallery { get; set; } = new List<IFormFile>();
    }
}
