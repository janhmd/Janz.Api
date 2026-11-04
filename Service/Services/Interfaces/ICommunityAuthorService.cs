using Service.Helpers.DTOs.Community;

namespace Service.Services.Interfaces
{
    public interface ICommunityAuthorService
    {
        // Müəlliflərin ictimai görünüşü: ləqəb (yoxdursa "Aysel M."), şəkil və "Səviyyə, Şəhər"
        Task<Dictionary<string, AuthorDto>> GetAsync(IEnumerable<string> userIds);
    }
}
