using Service.Helpers.DTOs.Accounts;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto model);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto model);
    }
}
