using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Products
{
    // Admin formasında bir variant (multipart: Variants[0].Format, Variants[0].NewImages ...)
    public class ProductVariantFormDto
    {
        public int? Id { get; set; }                 // mövcud variant; boşdursa yenidir
        public string? Format { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }

        // Saxlanılan köhnə şəkillər (sırası ilə) və əlavə olunan yeni şəkillər
        public List<string> KeepImages { get; set; } = new List<string>();
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();
    }
}
