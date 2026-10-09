using Service.DTOs.Account;
using Service.DTOs.Common;

namespace Service.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
}
