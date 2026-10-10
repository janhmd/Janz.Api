using Domain.Entities;
using Service.DTOs.Categories;
using Service.DTOs.Products;
using Service.DTOs.Tags;

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

        public static TagDto ToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug
            };
        }

        public static ProductDto ToDto(Product product, Category category, IEnumerable<Tag> tags, IEnumerable<ProductImage> images)
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
                CategorySlug = category.Slug,
                Images = images.OrderBy(m => m.Order).Select(m => m.ImagePath).ToList(),
                Tags = tags.Select(ToDto).ToList()
            };
        }
    }
}
