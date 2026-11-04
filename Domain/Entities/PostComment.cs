using Domain.Common;

namespace Domain.Entities
{
    // Postun şərhi. ParentId doludursa cavabdır (yalnız bir səviyyə)
    public class PostComment : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int? ParentId { get; set; }
        public PostComment? Parent { get; set; }

        public string Text { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsHidden { get; set; }

        public ICollection<PostCommentVote> Votes { get; set; } = new List<PostCommentVote>();
    }
}
