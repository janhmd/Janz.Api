using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Cart;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
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
            return Ok(await _cartService.GetAsync(UserId));
        }

        [HttpPost("items")]
        public async Task<IActionResult> Add([FromBody] AddCartItemDto model)
        {
            return ToResponse(await _cartService.AddAsync(UserId, model));
        }

        [HttpPut("items/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCartItemDto model)
        {
            return ToResponse(await _cartService.UpdateAsync(UserId, id, model));
        }

        [HttpDelete("items/{id:int}")]
        public async Task<IActionResult> Remove(int id)
        {
            return ToResponse(await _cartService.RemoveAsync(UserId, id));
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
