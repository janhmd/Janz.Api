using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Profiles
{
    public class UploadAvatarDto
    {
        public IFormFile? Image { get; set; }
    }
}
