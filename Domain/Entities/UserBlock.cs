using Domain.Common;

namespace Domain.Entities
{
    // Blacklist: UserId istifadəçi BlockedId istifadəçinin postlarını və şərhlərini görmür
    public class UserBlock : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public string BlockedId { get; set; } = string.Empty;
        public AppUser Blocked { get; set; } = null!;
    }
}
