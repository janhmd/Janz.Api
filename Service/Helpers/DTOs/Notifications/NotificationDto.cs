namespace Service.Helpers.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
