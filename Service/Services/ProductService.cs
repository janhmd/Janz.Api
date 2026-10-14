using Domain.Entities;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Categories;
using Service.Helpers.DTOs.Products;
using Service.Helpers.DTOs.Tags;
using Service.Helpers.Exceptions;
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
            return categories.Select(ToDto);
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

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                throw new NotFoundException();
            }

            var result = await MapProductsAsync(new List<Product> { product });
            var dto = result.FirstOrDefault();
            if (dto is null)
            {
                throw new NotFoundException();
            }

            return dto;
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

                result.Add(ToDto(product, category, productTagList, images[product.Id]));
            }

            return result;
        }

        private static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            };
        }

        private static TagDto ToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug
            };
        }

        private static ProductDto ToDto(Product product, Category category, IEnumerable<Tag> tags, IEnumerable<ProductImage> images)
        {
            var dto = new ProductDto();
            Fill(dto, product, category, tags, images);
            return dto;
        }

        private static void Fill(ProductDto dto, Product product, Category category, IEnumerable<Tag> tags, IEnumerable<ProductImage> images)
        {
            dto.Id = product.Id;
            dto.Name = product.Name;
            dto.Description = product.Description;
            dto.Price = product.Price;
            dto.Stock = product.Stock;
            dto.ImagePath = product.ImagePath;
            dto.CategoryId = category.Id;
            dto.CategoryName = category.Name;
            dto.CategorySlug = category.Slug;
            dto.Images = images.OrderBy(m => m.Order).Select(m => m.ImagePath).ToList();
            dto.Tags = tags.Select(ToDto).ToList();
        }
    }
}
