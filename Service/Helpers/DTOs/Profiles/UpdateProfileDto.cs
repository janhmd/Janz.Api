namespace Service.Helpers.DTOs.Profiles
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Nickname { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationAddress { get; set; }
        public string? Voen { get; set; }
        public string? TaxNumber { get; set; }
    }
}
