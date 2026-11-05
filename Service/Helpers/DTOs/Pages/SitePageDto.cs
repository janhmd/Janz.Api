namespace Service.Helpers.DTOs.Pages
{
    public class SitePageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool InTerms { get; set; }
        public bool InFooter { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
