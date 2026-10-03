using Microsoft.AspNetCore.Mvc;

namespace Janz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(Array.Empty<object>());
    }
}
