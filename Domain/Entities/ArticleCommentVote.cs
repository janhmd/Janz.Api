using Domain.Common;

namespace Domain.Entities
{
    // Şərhə səs: +1 bəyənmə, -1 bəyənməmə. Bir istifadəçi bir şərhə bir səs verir
    public class ArticleCommentVote : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int CommentId { get; set; }
        public ArticleComment Comment { get; set; } = null!;

        public int Value { get; set; }
    }
}
