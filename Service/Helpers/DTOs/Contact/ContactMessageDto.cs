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

        // Davam edən yazışma: cavabdan sonrakı yazılar (müştəri və support növbə ilə)
        public List<ContactEntryDto> Entries { get; set; } = new();
        public bool IsClosed { get; set; }              // müştəri "söhbəti bitir" basıb
        public DateTime? ClosedAt { get; set; }
        public bool CanFollowUp { get; set; }           // müştəri yenidən yaza və ya söhbəti bitirə bilər
    }
}
