using Microsoft.AspNetCore.Http;

namespace Service.Helpers.DTOs.Tiles
{
    // Admin: kart forması (multipart/form-data). Yeni kartda şəkil məcburidir, dəyişəndə boş qalsa köhnə şəkil qalır
    public class SaveSiteTileDto
    {
        public string? Placement { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public IFormFile? Image { get; set; }
    }
}
