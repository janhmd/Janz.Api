namespace Service.Helpers.DTOs.Contact
{
    // Admin panelin "Messages" bölməsində göstərilən mesaj
    public class ContactMessageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsMember { get; set; }              // sayta giriş edən müştəri göndərib
        public bool IsAnswered { get; set; }
        public string? Reply { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
