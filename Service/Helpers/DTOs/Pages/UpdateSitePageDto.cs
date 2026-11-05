namespace Service.Helpers.DTOs.Pages
{
    public class UpdateSitePageDto
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public bool InTerms { get; set; }
        public bool InFooter { get; set; }
    }
}
