using Domain.Common;

namespace Domain.Entities
{
    // Şikayət: ya post (PostId), ya şərh (CommentId) haqqında. Admin baxıb bağlayır
    public class PostReport : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int? PostId { get; set; }
        public Post? Post { get; set; }

        public int? CommentId { get; set; }
        public PostComment? Comment { get; set; }

        public string Reason { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
    }
}
