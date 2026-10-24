using Service.Helpers.DTOs.Products;

namespace Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(bool includeHidden = false);
        Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug);
        Task<ProductDto> GetByIdAsync(int id, bool includeHidden = false);

        // Admin panel
        Task<ProductDto> CreateAsync(CreateProductDto model);
        Task UpdateAsync(int id, UpdateProductDto model);
        Task DeleteAsync(int id);
    }
}
