using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Balance;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/balance")]
    [ApiController]
    [Authorize]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceService _balanceService;
        private readonly IPaymentService _paymentService;

        public BalanceController(IBalanceService balanceService, IPaymentService paymentService)
        {
            _balanceService = balanceService;
            _paymentService = paymentService;
        }

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _balanceService.GetAsync(UserId));
        }

        [HttpPost("certificate")]
        public async Task<IActionResult> RedeemCertificate([FromBody] RedeemCertificateDto model)
        {
            return ToResponse(await _balanceService.RedeemCertificateAsync(UserId, model));
        }

        [HttpPost("top-up")]
        public async Task<IActionResult> TopUp([FromBody] TopUpDto model)
        {
            return ToResponse(await _paymentService.CreateTopUpSessionAsync(UserId, model));
        }

        [HttpPost("top-up/confirm")]
        public async Task<IActionResult> ConfirmTopUp([FromBody] ConfirmTopUpDto model)
        {
            return ToResponse(await _paymentService.ConfirmTopUpAsync(UserId, model));
        }

        private static IActionResult ToResponse<T>(ServiceResult<T> result)
        {
            if (result.IsNotFound)
            {
                return new NotFoundObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            if (!result.IsSuccess)
            {
                return new BadRequestObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            return new OkObjectResult(result.Data);
        }
    }
}
