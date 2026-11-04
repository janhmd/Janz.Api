using Service.Helpers.DTOs.Community;

namespace Service.Services.Interfaces
{
    public interface ICommunityProfileService
    {
        // key: Nickname və ya istifadəçi id-si
        Task<CommunityProfileDto> GetAsync(string key, string? viewerId);
        Task<CommunityProfileDto> UpdateMineAsync(string userId, UpdateCommunityProfileDto model);
        Task FollowAsync(string userId, string targetId);
        Task UnfollowAsync(string userId, string targetId);
        Task<List<CommunityPersonDto>> GetFollowersAsync(string key, string? viewerId);
        Task<List<CommunityPersonDto>> GetFollowingAsync(string key, string? viewerId);
        Task BlockAsync(string userId, string targetId);
        Task UnblockAsync(string userId, string targetId);
        Task<List<CommunityPersonDto>> GetBlockedAsync(string userId);
    }
}
