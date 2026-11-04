using Service.Helpers.DTOs.Community;

namespace Service.Services.Interfaces
{
    public interface ICommunityAdminService
    {
        Task<IEnumerable<AdminPostDto>> GetPostsAsync();
        Task<IEnumerable<AdminPostCommentDto>> GetCommentsAsync();
        Task<IEnumerable<AdminReportDto>> GetReportsAsync();
        Task SetPostHiddenAsync(int id, SetHiddenDto model);
        Task SetCommentHiddenAsync(int id, SetHiddenDto model);
        Task DeletePostAsync(int id);
        Task DeleteCommentAsync(int id);
        Task ResolveReportAsync(int id);
    }
}
