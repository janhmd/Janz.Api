using Domain.Common;

namespace Domain.Entities
{
    // İzləmə: FollowerId istifadəçi FollowedId istifadəçini izləyir
    public class UserFollow : BaseEntity
    {
        public string FollowerId { get; set; } = string.Empty;
        public AppUser Follower { get; set; } = null!;

        public string FollowedId { get; set; } = string.Empty;
        public AppUser Followed { get; set; } = null!;
    }
}
