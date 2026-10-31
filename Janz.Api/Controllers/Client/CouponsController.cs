using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Səbət: promo kodu yoxlamaq (api/coupons/check?code=WELCOME10&total=45.50)
    [Route("api/coupons")]
    [ApiController]
    [Authorize]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] string? code, [FromQuery] decimal total)
        {
            return Ok(await _couponService.CheckAsync(code, total));
        }
    }
}
