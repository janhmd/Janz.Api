using Service.Helpers.DTOs.Addresses;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAllAsync(string userId);
        Task<ServiceResult<AddressDto>> GetByIdAsync(string userId, int id);
        Task<ServiceResult<AddressDto>> CreateAsync(string userId, CreateAddressDto model);
        Task<ServiceResult<AddressDto>> UpdateAsync(string userId, int id, UpdateAddressDto model);
        Task<ServiceResult<bool>> DeleteAsync(string userId, int id);
    }
}
