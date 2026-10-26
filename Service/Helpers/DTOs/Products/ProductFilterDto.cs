namespace Service.Helpers.DTOs.Products
{
    // Kataloqun filtrləri və axtarış (api/products?...). Hamısı istəyə bağlıdır, boş olan filtr tətbiq olunmur.
    // Siyahı olanlar (məs. acidity=low&acidity=high) "və ya" ilə, müxtəlif filtrlər "və" ilə birləşir
    public class ProductFilterDto
    {
        public string? Q { get; set; }                        // ad, təsvir, notlar və region üzrə axtarış
        public string? Category { get; set; }                 // kateqoriyanın slug-ı
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public List<string> Acidity { get; set; } = new();    // low, medium, high
        public List<string> Density { get; set; } = new();
        public List<string> Roast { get; set; } = new();      // light, medium, dark
        public List<string> Taste { get; set; } = new();      // chocolate, flowers, fruit, alcohol
        public List<string> Process { get; set; } = new();    // washed, natural, honey, funky, blend, other
        public List<string> Country { get; set; } = new();
        public List<string> Feature { get; set; } = new();    // blend, decaf, single, microlot, milk, limited
        public string? Tag { get; set; }                      // teqin slug-ı
        public string? Use { get; set; }                      // espresso, filter
        public string? Format { get; set; }                   // capsules, drip, ... (variantın paket növü)
        public string? Tab { get; set; }                      // recommended, popular, new, week, running-out, microlot
        public string? Sort { get; set; }                     // popular, new, price-asc, price-desc, rating
    }
}
