using Microsoft.AspNetCore.Mvc;

namespace Janz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("signin")]
    public IActionResult SignIn()
    {
        return Ok(new { ok = false, message = "Not connected yet" });
    }
}
