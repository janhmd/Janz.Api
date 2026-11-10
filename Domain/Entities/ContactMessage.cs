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

        // İlk cavabı yazan işçi (Support/Admin); profil şəkli və adı müştəriyə göstərilir
        public string? AnsweredById { get; set; }

        // Admin və ya Support "söhbəti bitir" basanda yazışma bağlanır: yeni yazı qəbul olunmur (müştəri bitirə bilməz).
        // Cavabdan sonra müştəri yenidən yaza bilər (ContactEntry), bağlanana qədər söhbət açıq qalır
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
