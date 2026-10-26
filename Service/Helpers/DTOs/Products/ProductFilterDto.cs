namespace Service.Helpers.DTOs.Products
{
    // Kataloqun axtarışı, əsas filtrləri və sıralaması (api/products?...). Boş olan filtr tətbiq olunmur
    public class ProductFilterDto
    {
        public string? Q { get; set; }                        // ad, təsvir, notlar və region üzrə axtarış
        public string? Category { get; set; }                 // kateqoriyanın slug-ı
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public string? Sort { get; set; }                     // popular, new, price-asc, price-desc, rating
    }
}
