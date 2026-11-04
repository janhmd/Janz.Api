using Domain.Common;

namespace Domain.Entities
{
    public class SavedPost : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
    }
}
