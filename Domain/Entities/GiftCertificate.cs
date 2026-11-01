using Domain.Common;

namespace Domain.Entities
{
    // Hədiyyə sertifikatı: admin yaradır, müştəri kodu daxil edəndə məbləğ balansına keçir (bir dəfə)
    public class GiftCertificate : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        public string? UsedById { get; set; }
        public AppUser? UsedBy { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
