using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Addresses;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/addresses")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
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
            return Ok(await _addressService.GetAllAsync(UserId));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            return ToResponse(await _addressService.GetByIdAsync(UserId, id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto model)
        {
            return ToResponse(await _addressService.CreateAsync(UserId, model));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressDto model)
        {
            return ToResponse(await _addressService.UpdateAsync(UserId, id, model));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return ToResponse(await _addressService.DeleteAsync(UserId, id));
        }

        private static IActionResult ToResponse<T>(ServiceResult<T> result)
        {
            if (result.IsNotFound)
            {
                return new NotFoundObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            if (!result.IsSuccess)
            {
                return new BadRequestObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            return new OkObjectResult(result.Data);
        }
    }
}
