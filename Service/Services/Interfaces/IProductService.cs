using Service.Helpers.DTOs.Products;

namespace Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug);
        Task<ProductDto> GetByIdAsync(int id);

        // Admin panel
        Task<ProductDto> CreateAsync(CreateProductDto model);
        Task UpdateAsync(int id, UpdateProductDto model);
        Task DeleteAsync(int id);
    }
}
