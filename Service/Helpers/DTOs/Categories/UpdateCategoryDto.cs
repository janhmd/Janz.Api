namespace Service.Helpers.DTOs.Categories
{
    // Admin: dəyişdirilən kateqoriya. Slug boş qalsa addan düzəldilir
    public class UpdateCategoryDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
