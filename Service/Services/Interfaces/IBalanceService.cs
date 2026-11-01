using Service.Helpers.DTOs.Balance;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IBalanceService
    {
        Task<BalanceDto> GetAsync(string userId);
        Task<ServiceResult<BalanceDto>> RedeemCertificateAsync(string userId, RedeemCertificateDto model);
    }
}
