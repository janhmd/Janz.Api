namespace Service.Helpers.DTOs.Tags
{
    // Admin: dəyişdirilən teq. Slug boş qalsa addan düzəldilir
    public class UpdateTagDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
