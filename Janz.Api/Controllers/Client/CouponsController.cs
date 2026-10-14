using Microsoft.AspNetCore.Mvc;

namespace Janz.Api.Controllers.Client
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(Array.Empty<object>());
        }
    }
}
