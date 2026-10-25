using Service.Helpers.DTOs.Accounts;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IAccountService
    {
        // Qeydiyyatdan sonra giriş yoxdur: əvvəl email təsdiqlənməlidir (Data = email)
        Task<ServiceResult<string>> RegisterAsync(RegisterDto model);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto model);
        Task<ServiceResult<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto model);
        Task ResendConfirmationAsync(EmailDto model);
        Task ForgotPasswordAsync(EmailDto model);
        Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto model);
    }
}
