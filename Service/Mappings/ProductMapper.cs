using Domain.Entities;
using Service.DTOs.Categories;
using Service.DTOs.Products;

namespace Service.Mappings
{
    public static class ProductMapper
    {
        public static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            };
        }

        public static ProductDto ToDto(Product product, Category category)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImagePath = product.ImagePath,
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategorySlug = category.Slug
            };
        }
    }
}
