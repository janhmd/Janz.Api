namespace Service.Helpers.DTOs.Pages
{
    // Header-dəki Terms menyusu və footer üçün qısa siyahı
    public class SitePageLinkDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool InTerms { get; set; }
        public bool InFooter { get; set; }
    }
}
