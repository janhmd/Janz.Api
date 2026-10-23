using System.Security.Claims;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // İstifadəçilər: siyahı və profil — Support da görür. Bloklamaq yalnız Admin və SuperAdmin-dir
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Support)]
    public class UsersController : ControllerBase
    {
        private const string BlockRoles = Roles.SuperAdmin + "," + Roles.Admin;

        private readonly IUserAdminService _userAdminService;

        public UsersController(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        private string StaffUserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        // ?role=Admin&blocked=true&search=aysel
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] bool? blocked, [FromQuery] string? search)
        {
            return Ok(await _userAdminService.GetAllAsync(role, blocked, search));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            return Ok(await _userAdminService.GetByIdAsync(id));
        }

        [HttpPut("{id}/block")]
        [Authorize(Roles = BlockRoles)]
        public async Task<IActionResult> Block([FromRoute] string id)
        {
            await _userAdminService.BlockAsync(id, StaffUserId, User.IsInRole(Roles.SuperAdmin));
            return NoContent();
        }

        [HttpPut("{id}/unblock")]
        [Authorize(Roles = BlockRoles)]
        public async Task<IActionResult> Unblock([FromRoute] string id)
        {
            await _userAdminService.UnblockAsync(id, User.IsInRole(Roles.SuperAdmin));
            return NoContent();
        }
    }
}
