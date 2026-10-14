using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _orderService.GetAllAsync(UserId));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            return ToResponse(await _orderService.GetByIdAsync(UserId, id));
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            return ToResponse(await _orderService.CreateFromCartAsync(UserId));
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
