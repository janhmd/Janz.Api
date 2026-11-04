using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Community
{
    // Şərh forması (multipart/form-data): mətn və ya şəkil, cavabdırsa ParentId
    public class CreatePostCommentDto
    {
        public string? Text { get; set; }
        public int? ParentId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
