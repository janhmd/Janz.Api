using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.GiftCertificates;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: hədiyyə sertifikatları. Yalnız Admin və SuperAdmin dəyişə bilər
    [Route("api/admin/gift-certificates")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    public class GiftCertificatesController : ControllerBase
    {
        private readonly IGiftCertificateService _certificateService;

        public GiftCertificatesController(IGiftCertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            return Ok(await _certificateService.GetAllAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _certificateService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGiftCertificateDto request)
        {
            return Ok(await _certificateService.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateGiftCertificateDto request)
        {
            await _certificateService.UpdateAsync(id, request);
            return NoContent();
        }

        // Sertifikatın kodu seçilən müştərilərin bildirişlərinə gedir
        [HttpPost("{id:int}/notify")]
        public async Task<IActionResult> Notify([FromRoute] int id, [FromBody] NotifyGiftCertificateDto request)
        {
            return Ok(new { sent = await _certificateService.NotifyAsync(id, request) });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _certificateService.DeleteAsync(id);
            return NoContent();
        }
    }
}
