using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Orders;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: sifarişlər. Support da baxır və statusu dəyişir (müştəriyə kömək üçün)
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Support)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ?status=Pending&search=aysel
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search)
        {
            return Ok(await _orderService.GetAllForAdminAsync(status, search));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _orderService.GetByIdForAdminAsync(id));
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromBody] UpdateOrderStatusDto request)
        {
            await _orderService.ChangeStatusAsync(id, request);
            return NoContent();
        }
    }
}
