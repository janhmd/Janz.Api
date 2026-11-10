using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Contact;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Əlaqə formu: giriş etməyənlər də yaza bilir, giriş edibsə mesaj hesabla bağlanır
    [Route("api/contact")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] CreateContactMessageDto request)
        {
            await _contactService.CreateAsync(request, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NoContent();
        }
    }
}
