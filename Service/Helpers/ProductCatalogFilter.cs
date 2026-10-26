using Service.Helpers.DTOs.Products;

namespace Service.Helpers
{
    // Kataloqun axtarışı, əsas filtrləri və sıralaması
    public static class ProductCatalogFilter
    {
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

        private static bool Has(string? text, string word) => text is not null && text.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}
