using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Products
{
    // Admin: məhsulu dəyişmək. Şəkillər istəyə bağlıdır: göndərilməsə köhnələri qalır
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();

        // Yeni əsas şəkil: göndərilsə köhnəsini əvəz edir
        public IFormFile? Image { get; set; }

        // Qalereyaya əlavə olunacaq şəkillər
        public List<IFormFile> Gallery { get; set; } = new List<IFormFile>();

        // Qalereyadan silinəcək şəkillərin yolları (ProductDto.Images-dən götürülür)
        public List<string> RemoveImages { get; set; } = new List<string>();
    }
}
