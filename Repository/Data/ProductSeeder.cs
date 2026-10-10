using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data
{
    public static class ProductSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Categories.AnyAsync())
            {
                return;
            }

            var coffee = new Category
            {
                Name = "Coffee",
                Slug = "coffee"
            };
            var tea = new Category
            {
                Name = "Tea",
                Slug = "tea"
            };
            var chocolate = new Category
            {
                Name = "Chocolate and syrups",
                Slug = "chocolate"
            };
            var sets = new Category
            {
                Name = "Sets",
                Slug = "sets"
            };

            await db.Categories.AddRangeAsync(coffee, tea, chocolate, sets);
            await db.SaveChangesAsync();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Floral",
                    Description = "A light coffee with notes of flowers, dark berries and citrus",
                    Price = 29.90m,
                    Stock = 48,
                    ImagePath = "/images/floral.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Guatemala Santiago",
                    Description = "A sweet coffee with notes of orange, dried fruit and milk chocolate",
                    Price = 27.50m,
                    Stock = 36,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Indonesia Sumatra Strawberry Karo",
                    Description = "A bright coffee with notes of strawberry, cherry, cream caramel and milk chocolate",
                    Price = 24.90m,
                    Stock = 18,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Costa Rica Reposado",
                    Description = "A bright coffee with notes of tangerine, mango, apricot and lemon drops",
                    Price = 24.20m,
                    Stock = 22,
                    ImagePath = "/images/floral.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Costa Rica Vista Al Santo",
                    Description = "A bright coffee with notes of passion fruit, peach, raspberry and dark chocolate",
                    Price = 24.90m,
                    Stock = 20,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Indonesia Sumatra Karo Funky",
                    Description = "A bright coffee with notes of pineapple, passion fruit, berry vermouth and dark chocolate",
                    Price = 24.70m,
                    Stock = 16,
                    ImagePath = "/images/floral.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Candy",
                    Description = "A dense coffee with notes of dark caramel, candied fruit and spices",
                    Price = 33.20m,
                    Stock = 40,
                    ImagePath = "/images/candy.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Ethiopia Yirgacheffe",
                    Description = "A sweet coffee with notes of dark berries, citrus and milk chocolate",
                    Price = 31.20m,
                    Stock = 32,
                    ImagePath = "/images/floral.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Brazil Cerrado",
                    Description = "A dense coffee with notes of roasted nuts, chocolate and caramel",
                    Price = 28.80m,
                    Stock = 44,
                    ImagePath = "/images/floral.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Colombia Magdalena",
                    Description = "A juicy coffee with notes of red apple, dark grapes and cocoa",
                    Price = 33.20m,
                    Stock = 28,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Brazil Iguassu",
                    Description = "A sweet coffee with notes of peanut, cocoa powder and lemon zest",
                    Price = 29.90m,
                    Stock = 30,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Batch Brew",
                    Description = "A sweet fruity coffee made for batch brew",
                    Price = 33.90m,
                    Stock = 26,
                    ImagePath = "/images/guatemala.png",
                    CategoryId = coffee.Id
                },
                new Product
                {
                    Name = "Set with coffee for espresso",
                    Description = "A set of 5 coffee packs for an espresso machine, Turkish coffee and a moka pot",
                    Price = 143.90m,
                    Stock = 12,
                    ImagePath = "/images/set-box.png",
                    CategoryId = sets.Id
                },
                new Product
                {
                    Name = "Set with coffee for filter",
                    Description = "A set of 5 coffee packs for a drip coffee maker, Turkish coffee, French press, pour-over, AeroPress or a cup",
                    Price = 151.90m,
                    Stock = 10,
                    ImagePath = "/images/set-box.png",
                    CategoryId = sets.Id
                },
                new Product
                {
                    Name = "Set with drip bags and a Stojo cup",
                    Description = "Three packs of drip bags and a foldable Stojo Ink cup in a gift box",
                    Price = 139.90m,
                    Stock = 8,
                    ImagePath = "/images/set-drip.png",
                    CategoryId = sets.Id
                },
                new Product
                {
                    Name = "Set with coffee and a French press",
                    Description = "A Timemore French press and four packs of bright coffee in a gift box",
                    Price = 235.90m,
                    Stock = 6,
                    ImagePath = "/images/set-french.png",
                    CategoryId = sets.Id
                },
                new Product
                {
                    Name = "Aroma kit",
                    Description = "A mixed pick from the catalog. Coffee aroma molecules.",
                    Price = 45.00m,
                    Stock = 14,
                    ImagePath = "/images/aroma-kit.png",
                    CategoryId = sets.Id
                }
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
        }
    }
}
