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
        private readonly IBaseRepository<Tag> _tagRepo;
        private readonly IBaseRepository<ProductTag> _productTagRepo;
        private readonly IBaseRepository<ProductImage> _productImageRepo;

        public ProductService(IBaseRepository<Product> productRepo,
                              IBaseRepository<Category> categoryRepo,
                              IBaseRepository<Tag> tagRepo,
                              IBaseRepository<ProductTag> productTagRepo,
                              IBaseRepository<ProductImage> productImageRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _productTagRepo = productTagRepo;
            _productImageRepo = productImageRepo;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return categories.Select(ProductMapper.ToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return await MapProductsAsync(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug)
        {
            var categories = await _categoryRepo.GetAllAsync();
            var category = categories.FirstOrDefault(m =>
                m.Slug.Equals(slug.Trim(), StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                return new List<ProductDto>();
            }

            var products = await _productRepo.GetAllAsync();
            return await MapProductsAsync(products.Where(m => m.CategoryId == category.Id));
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                return null;
            }

            var result = await MapProductsAsync(new List<Product> { product });
            return result.FirstOrDefault();
        }

        private async Task<List<ProductDto>> MapProductsAsync(IEnumerable<Product> products)
        {
            var categories = (await _categoryRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var tags = (await _tagRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var productTags = (await _productTagRepo.GetAllAsync()).ToLookup(m => m.ProductId);
            var images = (await _productImageRepo.GetAllAsync()).ToLookup(m => m.ProductId);

            var result = new List<ProductDto>();
            foreach (var product in products)
            {
                if (!categories.TryGetValue(product.CategoryId, out var category))
                {
                    continue;
                }

                var productTagList = productTags[product.Id]
                    .Where(m => tags.ContainsKey(m.TagId))
                    .Select(m => tags[m.TagId]);

                result.Add(ProductMapper.ToDto(product, category, productTagList, images[product.Id]));
            }

            return result;
        }
    }
}
