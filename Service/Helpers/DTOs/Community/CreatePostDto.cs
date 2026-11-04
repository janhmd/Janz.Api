using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Community
{
    // Yazı formu (multipart/form-data)
    public class CreatePostDto
    {
        public string? Text { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public string? Brew { get; set; }
        public string? Recipe { get; set; }
        public string? Dose { get; set; }
        public string? Water { get; set; }
        public string? Grind { get; set; }
        public string? Time { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
