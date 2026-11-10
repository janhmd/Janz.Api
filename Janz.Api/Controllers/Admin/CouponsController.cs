using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Coupons;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: kuponlar. Yalnız Admin və SuperAdmin dəyişə bilər
    [Route("api/admin/coupons")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            return Ok(await _couponService.GetAllAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _couponService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto request)
        {
            return Ok(await _couponService.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCouponDto request)
        {
            await _couponService.UpdateAsync(id, request);
            return NoContent();
        }

        // Kuponun kodu seçilən müştərilərin bildirişlərinə gedir
        [HttpPost("{id:int}/notify")]
        public async Task<IActionResult> Notify([FromRoute] int id, [FromBody] NotifyCouponDto request)
        {
            return Ok(new { sent = await _couponService.NotifyAsync(id, request) });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _couponService.DeleteAsync(id);
            return NoContent();
        }
    }
}
