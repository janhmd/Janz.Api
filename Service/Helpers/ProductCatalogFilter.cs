using Domain.Constants;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers
{
    // Kataloq filtrləri: məhsulun sahələrindən (turşuluq, emal, region, teqlər...) hesablanır
    public static class ProductCatalogFilter
    {
        private static readonly Dictionary<string, string> TasteKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            ["chocolate"] = TasteGroups.ChocolateNuts,
            ["flowers"] = TasteGroups.FlowersCitrus,
            ["fruit"] = TasteGroups.FruitsBerries,
            ["alcohol"] = TasteGroups.AlcoholFunky
        };

        private const int LowStock = 5;

        public static IEnumerable<ProductDto> Apply(IEnumerable<ProductDto> products, ProductFilterDto filter)
        {
            var query = products;

            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                var q = filter.Q.Trim();
                query = query.Where(m => Has(m.Name, q) || Has(m.Description, q) || Has(m.Region, q) || Has(m.Taste, q)
                    || m.Notes.Any(n => Has(n, q)) || m.Tags.Any(t => Has(t.Name, q)) || Has(m.CategoryName, q));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(m => m.CategorySlug.Equals(filter.Category.Trim(), StringComparison.OrdinalIgnoreCase));
            if (filter.MinPrice is decimal min) query = query.Where(m => m.Price >= min);
            if (filter.MaxPrice is decimal max) query = query.Where(m => m.Price <= max);
            if (filter.MinRating is decimal rating) query = query.Where(m => m.Rating >= rating);

            if (filter.Acidity.Count > 0) query = query.Where(m => m.Acidity is int v && filter.Acidity.Contains(Level(v), StringComparer.OrdinalIgnoreCase));
            if (filter.Density.Count > 0) query = query.Where(m => m.Density is int v && filter.Density.Contains(Level(v), StringComparer.OrdinalIgnoreCase));
            if (filter.Roast.Count > 0) query = query.Where(m => RoastOf(m.Roast) is string r && filter.Roast.Contains(r, StringComparer.OrdinalIgnoreCase));
            if (filter.Taste.Count > 0)
            {
                var groups = filter.Taste.Where(TasteKeys.ContainsKey).Select(k => TasteKeys[k]).ToList();
                query = query.Where(m => m.TasteGroup is not null && groups.Contains(m.TasteGroup));
            }
            if (filter.Process.Count > 0) query = query.Where(m => filter.Process.Contains(ProcessOf(m.Processing), StringComparer.OrdinalIgnoreCase));
            if (filter.Country.Count > 0) query = query.Where(m => filter.Country.Any(c => Has(m.Region, c.Replace('-', ' '))));
            foreach (var feature in filter.Feature) query = query.Where(m => HasFeature(m, feature));

            if (!string.IsNullOrWhiteSpace(filter.Tag)) query = query.Where(m => m.Tags.Any(t => t.Slug.Equals(filter.Tag.Trim(), StringComparison.OrdinalIgnoreCase)));
            if (!string.IsNullOrWhiteSpace(filter.Use)) query = query.Where(m => Has(m.Use, filter.Use.Trim()));
            if (!string.IsNullOrWhiteSpace(filter.Format))
            {
                var format = filter.Format.Trim().TrimEnd('s');
                query = query.Where(m => m.Variants.Any(v => Has(v.Format, format)));
            }

            query = filter.Tab?.ToLowerInvariant() switch
            {
                "recommended" => query.Where(m => m.Rating >= 4.5m),
                "popular" => query.Where(m => m.ReviewCount > 0).OrderByDescending(m => m.ReviewCount),
                "new" => query.Where(m => m.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                "week" => query.Where(m => m.Badge == ProductBadges.CoffeeOfTheWeek),
                "running-out" => query.Where(m => m.Stock > 0 && m.Stock < LowStock),
                "microlot" => query.Where(m => HasFeature(m, "microlot")),
                _ => query
            };

            return filter.Sort?.ToLowerInvariant() switch
            {
                "price-asc" => query.OrderBy(m => m.Price),
                "price-desc" => query.OrderByDescending(m => m.Price),
                "rating" => query.OrderByDescending(m => m.Rating ?? 0).ThenByDescending(m => m.ReviewCount),
                "popular" => query.OrderByDescending(m => m.ReviewCount),
                "new" => query.OrderByDescending(m => m.CreatedAt),
                _ => query
            };
        }

        // 0-33 aşağı, 34-66 orta, 67-100 yüksək
        public static string Level(int value) => value <= 33 ? "low" : value <= 66 ? "medium" : "high";

        private static string? RoastOf(string? roast)
        {
            if (string.IsNullOrWhiteSpace(roast)) return null;
            if (Has(roast, "light") || Has(roast, "filter")) return "light";
            if (Has(roast, "dark") || Has(roast, "milk")) return "dark";
            return "medium";
        }

        private static string ProcessOf(string? processing)
        {
            if (string.IsNullOrWhiteSpace(processing)) return "other";
            if (Has(processing, "blend")) return "blend";
            if (Has(processing, "anaerob") || Has(processing, "ferment") || Has(processing, "funky") || Has(processing, "carbonic")) return "funky";
            if (Has(processing, "honey")) return "honey";
            if (Has(processing, "natural")) return "natural";
            if (Has(processing, "washed")) return "washed";
            return "other";
        }

        private static bool HasFeature(ProductDto product, string feature)
        {
            bool tag(string word) => product.Tags.Any(t => Has(t.Name, word));
            return feature.ToLowerInvariant() switch
            {
                "blend" => ProcessOf(product.Processing) == "blend",
                "single" => !string.IsNullOrWhiteSpace(product.Processing) && ProcessOf(product.Processing) != "blend",
                "decaf" => Has(product.Name, "decaf") || tag("decaf") || (decimal.TryParse((product.Caffeine ?? string.Empty).TrimEnd('%').Replace(',', '.'),
                    System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var caffeine) && caffeine < 0.6m),
                "microlot" => tag("microlot") || Has(product.Name, "microlot"),
                "milk" => tag("milk") || Has(product.Use, "milk"),
                "limited" => product.Badge == ProductBadges.Limited || tag("limited"),
                _ => true
            };
        }

        private static bool Has(string? text, string word) => text is not null && text.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}
