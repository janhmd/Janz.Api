using Domain.Common;

namespace Domain.Entities
{
    // Məqaləni bəyənmək: bir istifadəçi bir dəfə
    public class ArticleLike : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
    }
}
