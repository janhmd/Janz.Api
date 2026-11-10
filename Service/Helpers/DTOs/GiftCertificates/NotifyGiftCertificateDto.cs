namespace Service.Helpers.DTOs.GiftCertificates
{
    // Admin: sertifikatın kodu kimlərə göndərilir. Audience: All (bütün müştərilər) və ya Selected (UserIds-dəkilər)
    public class NotifyGiftCertificateDto
    {
        public string? Audience { get; set; }
        public List<string>? UserIds { get; set; }
    }
}
