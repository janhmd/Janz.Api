namespace Service.Helpers.DTOs.Profiles
{
    // Kabinetdəki "My details": istifadəçinin öz məlumatları
    public class ProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Nickname { get; set; }
        public string? AvatarPath { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationAddress { get; set; }
        public string? Voen { get; set; }
        public string? TaxNumber { get; set; }
    }
}
