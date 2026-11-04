using Service.Helpers.DTOs.Community;

namespace Service.Services.Interfaces
{
    public interface IPostService
    {
        // Lent: tab "best" (səsə görə) və ya "new". Axtarış: scope "everywhere", "posts" və ya "comments"
        Task<IEnumerable<PostDto>> GetFeedAsync(string? tab, string? tag, string? search, string? scope, string? viewerId);

        // Bir istifadəçinin postları (gizli olanlar yoxdur)
        Task<List<PostDto>> GetByUserAsync(string authorId, string? viewerId);

        Task<IEnumerable<PostDto>> GetSavedAsync(string viewerId);
        Task<PostDto> CreateAsync(string userId, CreatePostDto model);
        Task DeleteAsync(string userId, int id);
        Task<VoteResultDto> VoteAsync(string userId, int id, PostVoteDto model);
        Task SaveAsync(string userId, int id, bool on);
        Task ReportAsync(string userId, int id, ReportPostDto model);
    }
}
