using Service.Helpers.DTOs.Community;

namespace Service.Services.Interfaces
{
    public interface IPostCommentService
    {
        Task<PostCommentDto> AddAsync(string userId, int postId, CreatePostCommentDto model);
        Task DeleteAsync(string userId, int id);
        Task<VoteResultDto> VoteAsync(string userId, int id, PostVoteDto model);
        Task ReportAsync(string userId, int id, ReportPostDto model);
    }
}
