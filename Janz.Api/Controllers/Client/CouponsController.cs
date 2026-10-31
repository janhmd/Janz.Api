using System.Security.Claims;
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

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] string? code, [FromQuery] decimal total)
        {
            return Ok(await _couponService.CheckAsync(code, total, UserId));
        }

        // Kupon yoxdursa 404: sayt kartı göstərmir
        [HttpGet("first-order")]
        public async Task<IActionResult> FirstOrder()
        {
            var coupon = await _couponService.GetFirstOrderAsync(UserId);
            return coupon is null ? NotFound() : Ok(coupon);
        }
    }
}
