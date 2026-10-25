namespace Service.Helpers.DTOs.Accounts
{
    // Emaildəki linkdən gələn istifadəçi və token (token base64url ilə kodlanıb)
    public class ConfirmEmailDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
    }

    // Təsdiq linkini yenidən göndərmək və şifrəni unutmaq üçün
    public class EmailDto
    {
        public string? Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
        public string? Password { get; set; }
        public string? RepeatPassword { get; set; }
    }
}
