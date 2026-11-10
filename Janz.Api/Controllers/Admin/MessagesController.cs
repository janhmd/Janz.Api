using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Contact;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: müştəri mesajları (əlaqə formu). Support da görür və cavab verir
    [Route("api/admin/messages")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Support)]
    public class MessagesController : ControllerBase
    {
        private readonly IContactService _contactService;

        public MessagesController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            return Ok(await _contactService.GetAllAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _contactService.GetByIdAsync(id));
        }

        // Söhbəti yalnız işçi (Admin/Support) bitirir
        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close([FromRoute] int id)
        {
            await _contactService.CloseAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/reply")]
        public async Task<IActionResult> Reply([FromRoute] int id, [FromBody] ReplyContactMessageDto request)
        {
            await _contactService.ReplyAsync(id, request);
            return NoContent();
        }
    }
}
