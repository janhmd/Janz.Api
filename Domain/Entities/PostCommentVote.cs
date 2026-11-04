using Domain.Common;

namespace Domain.Entities
{
    public class PostCommentVote : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int CommentId { get; set; }
        public PostComment Comment { get; set; } = null!;

        public int Value { get; set; }
    }
}
