using Domain.Common;

namespace Domain.Entities
{
    // Posta səs: +1 bəyənmə, -1 bəyənməmə. Bir istifadəçi bir posta bir səs verir
    public class PostVote : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int Value { get; set; }
    }
}
