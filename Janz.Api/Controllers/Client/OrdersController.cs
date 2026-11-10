using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Service.Helpers;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.DTOs.Payments;
using Service.Helpers.DTOs.Refunds;
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
        private readonly IRefundService _refundService;
        private readonly DeliveryRules _delivery;

        public OrdersController(IOrderService orderService, IPaymentService paymentService, IRefundService refundService, IConfiguration configuration)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _refundService = refundService;
            _delivery = DeliveryRules.From(configuration);
        }

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        // Səbət səhifəsi üçün çatdırılma haqqı və pulsuz çatdırılma həddi
        [HttpGet("delivery")]
        [AllowAnonymous]
        public IActionResult GetDelivery()
        {
            return Ok(new DeliveryRuleDto { Fee = _delivery.Fee, FreeFrom = _delivery.FreeFrom });
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

        // Kart ödənişi ləğv edildi/uğursuz oldu: sifariş silinir, məhsullar səbətdə qalır (ödənibsə sifariş yaranır)
        [HttpPost("{id:int}/abort-payment")]
        public async Task<IActionResult> AbortPayment(int id)
        {
            return ToResponse(await _paymentService.AbortAsync(UserId, id));
        }

        // Çatdırılmış sifariş üçün pulu geri istəmək: Admin və ya Support qəbul edəndə məbləğ balansa qayıdır
        [HttpPost("{id:int}/refund")]
        public async Task<IActionResult> RequestRefund(int id, [FromBody] CreateRefundDto model)
        {
            return ToResponse(await _refundService.CreateAsync(UserId, id, model));
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
