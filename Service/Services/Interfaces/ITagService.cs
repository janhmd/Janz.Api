using Service.Helpers.DTOs.Tags;

namespace Service.Services.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllAsync();
        Task<TagDto> GetByIdAsync(int id);

        // Admin panel
        Task<TagDto> CreateAsync(CreateTagDto model);
        Task UpdateAsync(int id, UpdateTagDto model);
        Task DeleteAsync(int id);
    }
}
