namespace Service.Helpers.DTOs.Categories
{
    // Admin: yeni kateqoriya. Slug boş qalsa addan düzəldilir
    public class CreateCategoryDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
