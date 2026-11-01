using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Seçilmişlər: məhsulların Id-ləri qaytarılır, sayt məhsulları kataloqdan götürür
    [Route("api/wishlist")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
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
            return Ok(await _wishlistService.GetProductIdsAsync(UserId));
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add([FromRoute] int productId)
        {
            await _wishlistService.AddAsync(UserId, productId);
            return NoContent();
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove([FromRoute] int productId)
        {
            await _wishlistService.RemoveAsync(UserId, productId);
            return NoContent();
        }
    }
}
