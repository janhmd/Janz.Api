using Domain.Common;

namespace Domain.Entities
{
    // Terms menyusundakı və footer-dəki mətn səhifələri (/Pages/{slug})
    public class SitePage : BaseEntity
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool InTerms { get; set; }
        public bool InFooter { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
