using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data
{
    public static class ProductImageSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.ProductImages.AnyAsync())
            {
                return;
            }

            var floral = await db.Products.FirstOrDefaultAsync(m => m.Name == "Floral");
            if (floral is null)
            {
                return;
            }

            var images = new List<ProductImage>
            {
                new ProductImage { ImagePath = "/images/floral.png", Order = 0, ProductId = floral.Id },
                new ProductImage { ImagePath = "/images/floral-bag.png", Order = 1, ProductId = floral.Id },
                new ProductImage { ImagePath = "/images/floral-beans.jpg", Order = 2, ProductId = floral.Id }
            };

            await db.ProductImages.AddRangeAsync(images);
            await db.SaveChangesAsync();
        }
    }
}
