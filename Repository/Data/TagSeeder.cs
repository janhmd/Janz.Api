using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data
{
    public static class TagSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Tags.AnyAsync())
            {
                return;
            }

            var tags = new List<Tag>
            {
                new Tag { Name = "For an espresso machine", Slug = "espresso-machine" },
                new Tag { Name = "For Turkish coffee", Slug = "turkish-coffee" },
                new Tag { Name = "For milk drinks", Slug = "milk-drinks" },
                new Tag { Name = "For a portafilter", Slug = "portafilter" },
                new Tag { Name = "Ground coffee", Slug = "ground-coffee" },
                new Tag { Name = "Coffee with low acidity", Slug = "low-acidity" },
                new Tag { Name = "For a cup", Slug = "for-a-cup" },
                new Tag { Name = "Fermented coffee", Slug = "fermented" },
                new Tag { Name = "Limited edition", Slug = "limited-edition" },
                new Tag { Name = "Whole bean", Slug = "whole-bean" },
                new Tag { Name = "Whole bean 1 kg", Slug = "whole-bean-1-kg" }
            };

            await db.Tags.AddRangeAsync(tags);
            await db.SaveChangesAsync();

            var tagsBySlug = tags.ToDictionary(m => m.Slug);
            var products = await db.Products.ToDictionaryAsync(m => m.Name);
            var coffee = await db.Categories.FirstOrDefaultAsync(m => m.Slug == "coffee");

            var links = new Dictionary<string, string[]>
            {
                { "Floral", new[] { "espresso-machine", "turkish-coffee", "for-a-cup" } },
                { "Guatemala Santiago", new[] { "turkish-coffee", "for-a-cup" } },
                { "Indonesia Sumatra Strawberry Karo", new[] { "turkish-coffee", "limited-edition" } },
                { "Costa Rica Reposado", new[] { "espresso-machine", "limited-edition" } },
                { "Costa Rica Vista Al Santo", new[] { "for-a-cup", "limited-edition" } },
                { "Indonesia Sumatra Karo Funky", new[] { "espresso-machine", "turkish-coffee", "fermented", "limited-edition" } },
                { "Candy", new[] { "espresso-machine", "turkish-coffee", "milk-drinks", "low-acidity" } },
                { "Ethiopia Yirgacheffe", new[] { "espresso-machine", "for-a-cup" } },
                { "Brazil Cerrado", new[] { "espresso-machine", "low-acidity" } },
                { "Batch Brew", new[] { "for-a-cup" } }
            };

            var productTags = new List<ProductTag>();
            foreach (var product in products.Values)
            {
                var slugs = new List<string>();
                if (links.TryGetValue(product.Name, out var linked))
                {
                    slugs.AddRange(linked);
                }

                if (coffee is not null && product.CategoryId == coffee.Id)
                {
                    slugs.Add("whole-bean");
                    slugs.Add("whole-bean-1-kg");
                    slugs.Add("ground-coffee");
                }

                foreach (var slug in slugs.Distinct())
                {
                    productTags.Add(new ProductTag
                    {
                        ProductId = product.Id,
                        TagId = tagsBySlug[slug].Id
                    });
                }
            }

            await db.ProductTags.AddRangeAsync(productTags);
            await db.SaveChangesAsync();
        }
    }
}
