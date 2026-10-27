namespace Service.Helpers.DTOs.Accounts
{
    // Emaildəki linkdən gələn istifadəçi və token (token base64url ilə kodlanıb)
    public class ConfirmEmailDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
    }
}
