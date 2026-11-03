namespace Service.Helpers.DTOs.Notifications
{
    public class NotificationListDto
    {
        public int Unread { get; set; }
        public List<NotificationDto> Items { get; set; } = new();
    }
}
