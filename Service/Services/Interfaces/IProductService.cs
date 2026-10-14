using Service.Helpers.DTOs.Categories;
using Service.Helpers.DTOs.Products;

namespace Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug);
        Task<ProductDto> GetByIdAsync(int id);
    }
}
