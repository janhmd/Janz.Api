using Service.Helpers.DTOs.Articles;

namespace Service.Services.Interfaces
{
    public interface IArticleService
    {
        // Sayt: yalnız dərc olunmuşlar. sort: new (default), old, read, popular
        Task<IEnumerable<ArticleCardDto>> GetPublishedAsync(string? topic, string? tag, string? sort, string? userId);
        Task<ArticleDto> GetBySlugAsync(string slug, string? userId);

        Task<int> LikeAsync(int id, string userId, bool like);
        Task SaveAsync(int id, string userId, bool save);
        Task<IEnumerable<ArticleCardDto>> GetSavedAsync(string userId);

        Task<IEnumerable<ArticleCommentDto>> GetCommentsAsync(int id, string? userId);
        Task<ArticleCommentDto> AddCommentAsync(int id, string userId, CreateArticleCommentDto model);
        Task DeleteCommentAsync(int commentId, string userId, bool isStaff);
        Task<ArticleCommentDto> VoteAsync(int commentId, string userId, ArticleVoteDto model);

        // Admin panel
        Task<IEnumerable<ArticleAdminDto>> GetAllAsync();
        Task<ArticleAdminDto> GetByIdAsync(int id);
        Task<ArticleAdminDto> CreateAsync(CreateArticleDto model);
        Task UpdateAsync(int id, UpdateArticleDto model);
        Task DeleteAsync(int id);
    }
}
