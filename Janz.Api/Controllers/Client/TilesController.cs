using Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Home və kataloq səhifələrindəki kartlar hamıya açıqdır
    [Route("api/tiles")]
    [ApiController]
    public class TilesController : ControllerBase
    {
        private readonly ISiteTileService _tileService;

        public TilesController(ISiteTileService tileService)
        {
            _tileService = tileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string placement)
        {
            if (!TilePlacements.All.Contains(placement))
            {
                return Ok(Array.Empty<object>());
            }

            return Ok(await _tileService.GetVisibleAsync(placement));
        }
    }
}
