namespace Service.Helpers.DTOs.Tiles
{
    public class SiteTileDto
    {
        public int Id { get; set; }
        public string Placement { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string? VideoPath { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }
}
