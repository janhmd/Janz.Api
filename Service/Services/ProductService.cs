using Domain.Entities;
using Repository.Repositories.Interfaces;
using Service.DTOs.Categories;
using Service.DTOs.Products;
using Service.Mappings;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<Category> _categoryRepo;

        public ProductService(IBaseRepository<Product> productRepo, IBaseRepository<Category> categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return categories.Select(ProductMapper.ToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            var categories = await GetCategoryMapAsync();
            return MapProducts(products, categories);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug)
        {
            var categories = await GetCategoryMapAsync();
            var category = categories.Values.FirstOrDefault(m =>
                m.Slug.Equals(slug.Trim(), StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                return new List<ProductDto>();
            }

            var products = await _productRepo.GetAllAsync();
            return MapProducts(products.Where(m => m.CategoryId == category.Id), categories);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                return null;
            }

            var category = await _categoryRepo.GetByIdAsync(product.CategoryId);
            if (category is null)
            {
                return null;
            }

            return ProductMapper.ToDto(product, category);
        }

        private async Task<Dictionary<int, Category>> GetCategoryMapAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return categories.ToDictionary(m => m.Id);
        }

        private static IEnumerable<ProductDto> MapProducts(IEnumerable<Product> products, Dictionary<int, Category> categories)
        {
            var result = new List<ProductDto>();
            foreach (var product in products)
            {
                if (!categories.TryGetValue(product.CategoryId, out var category))
                {
                    continue;
                }

                result.Add(ProductMapper.ToDto(product, category));
            }

            return result;
        }
    }
}
