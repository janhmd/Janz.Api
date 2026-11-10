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
        public string? UserId { get; set; }
        public string? UserAvatar { get; set; }
        public bool IsMember { get; set; }              // sayta giriş edən müştəri göndərib
        public bool IsAnswered { get; set; }
        public string? Reply { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public string? AnsweredByName { get; set; }
        public string? AnsweredByAvatar { get; set; }
        public DateTime CreatedAt { get; set; }

        // Davam edən yazışma: cavabdan sonrakı yazılar (müştəri və support növbə ilə)
        public List<ContactEntryDto> Entries { get; set; } = new();
        public bool IsClosed { get; set; }              // Admin/Support söhbəti bitirib
        public DateTime? ClosedAt { get; set; }
        public bool CanFollowUp { get; set; }           // müştəri yenidən yaza bilər
    }
}
