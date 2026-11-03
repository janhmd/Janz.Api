using Domain.Common;

namespace Domain.Entities
{
    // Saxlanmış məqalə: seçilmişlərin "Notes" tabında görünür
    public class SavedArticle : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
    }
}
