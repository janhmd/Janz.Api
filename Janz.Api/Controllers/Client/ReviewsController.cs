using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Reviews;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Mağaza rəyləri: siyahı hamıya açıqdır, rəyi yalnız sifarişin sahibi yazır (sifariş çatdırılandan sonra)
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ?skip=0&take=20&shop=5,4&delivery=5&search=fast
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int skip = 0, [FromQuery] int take = 20, [FromQuery] string? shop = null, [FromQuery] string? delivery = null, [FromQuery] string? search = null)
        {
            return Ok(await _reviewService.GetShopReviewsAsync(skip, take, Stars(shop), Stars(delivery), search));
        }

        // "5,4" -> [5, 4]; yalnız 1-5 qəbul edilir
        private static List<int> Stars(string? value)
        {
            return (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(m => int.TryParse(m, out var star) ? star : 0)
                .Where(m => m is >= 1 and <= 5)
                .Distinct()
                .ToList();
        }

        [HttpGet("order/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> GetForOrder([FromRoute] int orderId)
        {
            return Ok(await _reviewService.GetForOrderAsync(orderId, UserId));
        }

        [HttpPut("order/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> SaveForOrder([FromRoute] int orderId, [FromBody] SaveShopReviewDto model)
        {
            return Ok(await _reviewService.SaveForOrderAsync(orderId, UserId, model));
        }
    }
}
