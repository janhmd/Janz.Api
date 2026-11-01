using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Balance;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceRepository _balanceRepo;
        private readonly IValidator<RedeemCertificateDto> _redeemValidator;

        public BalanceService(IBalanceRepository balanceRepo,
                              IValidator<RedeemCertificateDto> redeemValidator)
        {
            _balanceRepo = balanceRepo;
            _redeemValidator = redeemValidator;
        }

        public async Task<BalanceDto> GetAsync(string userId)
        {
            var transactions = await _balanceRepo.GetByUserAsync(userId);
            return new BalanceDto
            {
                Balance = transactions.Sum(m => m.Amount),
                Transactions = transactions.Select(m => new BalanceTransactionDto
                {
                    Id = m.Id,
                    Amount = m.Amount,
                    Type = m.Type.ToString(),
                    Note = m.Note,
                    OrderId = m.OrderId,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
        }

        public async Task<ServiceResult<BalanceDto>> RedeemCertificateAsync(string userId, RedeemCertificateDto model)
        {
            var validation = await _redeemValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<BalanceDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var entry = await _balanceRepo.RedeemCertificateAsync(userId, model.Code.Trim().ToUpperInvariant());
            if (entry is null)
            {
                return ServiceResult<BalanceDto>.Fail("This certificate is not valid, expired or already used.");
            }

            return ServiceResult<BalanceDto>.Ok(await GetAsync(userId));
        }
    }
}
