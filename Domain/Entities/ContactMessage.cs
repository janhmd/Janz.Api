using Domain.Common;

namespace Domain.Entities
{
    // Müştərinin saytdakı əlaqə formundan göndərdiyi mesaj (sual, şikayət). Giriş edibsə UserId yazılır.
    // Admin və ya Support cavab yazanda cavab e-poçtla göndərilir və mesaj "answered" olur
    public class ContactMessage : BaseEntity
    {
        public string? UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Order | Payment | Product | Complaint | Other (ContactTopics)
        public string Topic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool IsAnswered { get; set; }
        public string? Reply { get; set; }
        public DateTime? AnsweredAt { get; set; }
    }
}
