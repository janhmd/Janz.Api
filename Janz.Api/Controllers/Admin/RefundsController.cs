using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Refunds;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: pulun geri qaytarılması sorğuları. Admin və Support qəbul edir (balansa qayıdır) və ya rədd edir
    [Route("api/admin/refunds")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Support)]
    public class RefundsController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundsController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        // ?status=Pending
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            return Ok(await _refundService.GetAllForAdminAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _refundService.GetByIdForAdminAsync(id));
        }

        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve([FromRoute] int id, [FromBody] ResolveRefundDto request)
        {
            await _refundService.ApproveAsync(id, request);
            return NoContent();
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] ResolveRefundDto request)
        {
            await _refundService.RejectAsync(id, request);
            return NoContent();
        }
    }
}
