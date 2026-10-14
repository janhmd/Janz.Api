using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Cart;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _cartService.GetAsync(GetUserId()));
        }

        [HttpPost("items")]
        public async Task<IActionResult> Add(AddCartItemDto dto)
        {
            var result = await _cartService.AddAsync(GetUserId(), dto);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpPut("items/{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateCartItemDto dto)
        {
            var result = await _cartService.UpdateAsync(GetUserId(), id, dto);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpDelete("items/{id:int}")]
        public async Task<IActionResult> Remove(int id)
        {
            var result = await _cartService.RemoveAsync(GetUserId(), id);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
