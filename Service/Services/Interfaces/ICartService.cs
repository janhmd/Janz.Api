using Service.DTOs.Cart;
using Service.DTOs.Common;

namespace Service.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetAsync(string userId);
        Task<ServiceResult<CartDto>> AddAsync(string userId, AddCartItemDto dto);
        Task<ServiceResult<CartDto>> UpdateAsync(string userId, int itemId, UpdateCartItemDto dto);
        Task<ServiceResult<CartDto>> RemoveAsync(string userId, int itemId);
    }
}
