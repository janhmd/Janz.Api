using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Rollar səhifəsi: yalnız SuperAdmin görür (rol verən də odur)
    [Route("api/admin/roles")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class RolesController : ControllerBase
    {
        private readonly IUserAdminService _userAdminService;

        public RolesController(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userAdminService.GetRolesAsync());
        }
    }
}
