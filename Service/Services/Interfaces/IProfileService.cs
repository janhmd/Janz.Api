using Service.Helpers.DTOs.Profiles;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResult<ProfileDto>> GetAsync(string userId);
        Task<ServiceResult<ProfileDto>> UpdateAsync(string userId, UpdateProfileDto model);
        Task<ServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordDto model);
        Task<ServiceResult<ProfileDto>> UploadAvatarAsync(string userId, UploadAvatarDto model);
        Task<ServiceResult<ProfileDto>> RemoveAvatarAsync(string userId);
    }
}
