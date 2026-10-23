namespace Service.Helpers.DTOs.Tags
{
    // Admin: yeni teq. Slug boş qalsa addan düzəldilir
    public class CreateTagDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
