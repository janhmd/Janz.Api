using Service.Helpers.DTOs.Cart;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetAsync(string userId);
        Task<ServiceResult<CartDto>> AddAsync(string userId, AddCartItemDto model);
        Task<ServiceResult<CartDto>> UpdateAsync(string userId, int itemId, UpdateCartItemDto model);
        Task<ServiceResult<CartDto>> RemoveAsync(string userId, int itemId);
    }
}
