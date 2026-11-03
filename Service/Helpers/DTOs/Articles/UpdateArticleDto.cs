using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Articles
{
    // Admin: məqalə forması (multipart/form-data)
    public class UpdateArticleDto
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Topic { get; set; }
        public string? Lead { get; set; }
        public string? Body { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string? AuthorName { get; set; }
        public string? AuthorRole { get; set; }
        public bool IsLearning { get; set; }
        public string? Series { get; set; }
        public bool IsPublished { get; set; }

        // Üz qabığı: yeni seçilməyibsə köhnəsi qalır
        public IFormFile? Cover { get; set; }

        // Mətndə "[photo N]" ilə qoyulan şəkillər: yenilər sona əlavə olunur
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        // Silinəcək köhnə şəkillərin yolları
        public List<string> RemoveImages { get; set; } = new List<string>();
    }
}
