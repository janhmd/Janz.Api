using Service.DTOs.Categories;
using Service.DTOs.Products;

namespace Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug);
        Task<ProductDto?> GetByIdAsync(int id);
    }
}
