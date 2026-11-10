using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.DTOs.Payments;
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
        private readonly IPaymentService _paymentService;

        public OrdersController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
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
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _orderService.GetAllAsync(UserId));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            await _paymentService.RefreshAsync(UserId, id);
            return ToResponse(await _orderService.GetByIdAsync(UserId, id));
        }

        // Səbətdəki hər şey seçilən ünvana sifariş olunur
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto model)
        {
            return ToResponse(await _orderService.CreateFromCartAsync(UserId, model));
        }

        // Kartla ödəniş: Stripe-ın ödəniş səhifəsinin ünvanı qaytarılır
        [HttpPost("{id:int}/checkout")]
        public async Task<IActionResult> Checkout(int id)
        {
            return ToResponse(await _paymentService.CreateSessionAsync(UserId, id));
        }

        // Stripe-dan qayıdanda: sifariş ödənibmi
        [HttpPost("{id:int}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentDto model)
        {
            return ToResponse(await _paymentService.ConfirmAsync(UserId, id, model));
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
