using Domain.Common;

namespace Domain.Entities
{
    public class ArticleComment : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        public string Text { get; set; } = string.Empty;

        public ICollection<ArticleCommentVote> Votes { get; set; } = new List<ArticleCommentVote>();
    }
}
