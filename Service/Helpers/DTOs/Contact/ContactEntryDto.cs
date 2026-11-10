namespace Service.Helpers.DTOs.Contact
{
    // Yazışmanın davamı: ilk mesaj və ilk cavabdan sonrakı hər yazı
    public class ContactEntryDto
    {
        public bool IsStaff { get; set; }
        public string? StaffName { get; set; }
        public string? StaffAvatar { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
