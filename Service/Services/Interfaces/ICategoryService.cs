using Service.Helpers.DTOs.Categories;

namespace Service.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);

        // Admin panel
        Task<CategoryDto> CreateAsync(CreateCategoryDto model);
        Task UpdateAsync(int id, UpdateCategoryDto model);
        Task DeleteAsync(int id);
    }
}
