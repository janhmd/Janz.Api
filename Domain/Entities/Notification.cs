using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    // İstifadəçiyə bildiriş: sifariş statusu və s. Oxunmayanlar header-dəki zəngdə sayılır
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public NotificationKind Kind { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        // Bildirişə basanda açılan səhifə (məsələn /Orders/Detail/5)
        public string? Link { get; set; }
        public bool IsRead { get; set; }
    }
}
