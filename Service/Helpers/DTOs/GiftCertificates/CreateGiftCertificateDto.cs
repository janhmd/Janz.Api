namespace Service.Helpers.DTOs.GiftCertificates
{
    public class CreateGiftCertificateDto
    {
        public string? Code { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
