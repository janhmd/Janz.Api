using Domain.Common;

namespace Domain.Entities
{
    // Əlaqə yazışmasının davamı: ilk mesaj və ilk cavab ContactMessage-dədir, ondan sonrakı hər yazı burada saxlanılır.
    // IsStaff = true: Admin/Support yazıb, false: müştəri cavabdan sonra yenidən yazıb
    public class ContactEntry : BaseEntity
    {
        public int ContactMessageId { get; set; }
        public ContactMessage ContactMessage { get; set; } = null!;

        public bool IsStaff { get; set; }

        // IsStaff = true olanda yazan işçi
        public string? StaffUserId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
