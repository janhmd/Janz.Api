using Domain.Common;

namespace Domain.Entities
{
    // Community postu: mətn, 0 və ya çox şəkil, istəyə görə məhsul (rəy və resept ilə) və teqlər
    public class Post : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public string Text { get; set; } = string.Empty;

        // Məhsul seçiləndə: qiymət (1-5) və hazırlama üsulu
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int? Rating { get; set; }
        public string? Brew { get; set; }
        public string? Recipe { get; set; }
        public string? Dose { get; set; }
        public string? Water { get; set; }
        public string? Grind { get; set; }
        public string? Time { get; set; }

        // Vergüllə ayrılmış teqlər (Article-dakı kimi)
        public string? Tags { get; set; }

        // Admin gizlədəndə lentdə görünmür
        public bool IsHidden { get; set; }

        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public ICollection<PostVote> Votes { get; set; } = new List<PostVote>();
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    }
}
