namespace Service.Helpers.DTOs.Accounts
{
    public class ResetPasswordDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
        public string? Password { get; set; }
        public string? RepeatPassword { get; set; }
    }
}
