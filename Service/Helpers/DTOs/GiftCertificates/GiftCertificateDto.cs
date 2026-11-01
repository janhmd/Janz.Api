namespace Service.Helpers.DTOs.GiftCertificates
{
    // Admin: sertifikat. Status: Active / Used / Expired / Disabled
    public class GiftCertificateDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? UsedByEmail { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
