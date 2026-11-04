using Domain.Entities;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Community;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CommunityAdminService : ICommunityAdminService
    {
        private readonly IBaseRepository<Post> _postRepo;
        private readonly IBaseRepository<PostComment> _commentRepo;
        private readonly IBaseRepository<PostVote> _voteRepo;
        private readonly IBaseRepository<PostReport> _reportRepo;
        private readonly PostService _postService;
        private readonly PostCommentService _commentService;
        private readonly ICommunityAuthorService _authors;

        public CommunityAdminService(IBaseRepository<Post> postRepo,
                                     IBaseRepository<PostComment> commentRepo,
                                     IBaseRepository<PostVote> voteRepo,
                                     IBaseRepository<PostReport> reportRepo,
                                     IPostService postService,
                                     IPostCommentService commentService,
                                     ICommunityAuthorService authors)
        {
            _postRepo = postRepo;
            _commentRepo = commentRepo;
            _voteRepo = voteRepo;
            _reportRepo = reportRepo;
            _postService = (PostService)postService;
            _commentService = (PostCommentService)commentService;
            _authors = authors;
        }

        public async Task<IEnumerable<AdminPostDto>> GetPostsAsync()
        {
            var posts = (await _postRepo.GetAllAsync()).ToList();
            var votes = (await _voteRepo.GetAllAsync()).ToList();
            var comments = (await _commentRepo.GetAllAsync()).ToList();
            var reports = (await _reportRepo.GetAllAsync()).Where(m => m.PostId.HasValue && !m.IsResolved).ToList();
            var authors = await _authors.GetAsync(posts.Select(m => m.UserId));

            return posts.Select(m => new AdminPostDto
            {
                Id = m.Id,
                Author = authors[m.UserId],
                Text = m.Text,
                IsHidden = m.IsHidden,
                Reports = reports.Count(r => r.PostId == m.Id),
                Score = votes.Where(v => v.PostId == m.Id).Sum(v => v.Value),
                CommentCount = comments.Count(c => c.PostId == m.Id),
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<IEnumerable<AdminPostCommentDto>> GetCommentsAsync()
        {
            var comments = (await _commentRepo.GetAllAsync()).ToList();
            var posts = (await _postRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var authors = await _authors.GetAsync(comments.Select(m => m.UserId));

            return comments.Select(m => new AdminPostCommentDto
            {
                Id = m.Id,
                PostId = m.PostId,
                Author = authors[m.UserId],
                Text = m.Text,
                PostText = posts.TryGetValue(m.PostId, out var post) ? post.Text : string.Empty,
                IsHidden = m.IsHidden,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<IEnumerable<AdminReportDto>> GetReportsAsync()
        {
            var reports = (await _reportRepo.GetAllAsync()).ToList();
            var posts = (await _postRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var comments = (await _commentRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var people = reports.Select(m => m.UserId)
                .Concat(reports.Where(m => m.PostId.HasValue && posts.ContainsKey(m.PostId.Value)).Select(m => posts[m.PostId!.Value].UserId))
                .Concat(reports.Where(m => m.CommentId.HasValue && comments.ContainsKey(m.CommentId.Value)).Select(m => comments[m.CommentId!.Value].UserId));
            var authors = await _authors.GetAsync(people);

            return reports.Select(m =>
            {
                string text = string.Empty;
                string author = string.Empty;
                if (m.PostId.HasValue && posts.TryGetValue(m.PostId.Value, out var post))
                {
                    text = post.Text;
                    author = authors[post.UserId].Name;
                }
                else if (m.CommentId.HasValue && comments.TryGetValue(m.CommentId.Value, out var comment))
                {
                    text = comment.Text;
                    author = authors[comment.UserId].Name;
                }

                return new AdminReportDto
                {
                    Id = m.Id,
                    Reporter = authors[m.UserId].Name,
                    Reason = m.Reason,
                    Target = m.PostId.HasValue ? "Post" : "Comment",
                    PostId = m.PostId,
                    CommentId = m.CommentId,
                    TargetText = text,
                    TargetAuthor = author,
                    IsResolved = m.IsResolved,
                    CreatedAt = m.CreatedAt
                };
            }).ToList();
        }

        public async Task SetPostHiddenAsync(int id, SetHiddenDto model)
        {
            var post = await _postRepo.GetByIdAsync(id) ?? throw new NotFoundException();
            post.IsHidden = model.IsHidden;
            await _postRepo.UpdateAsync(post);
        }

        public async Task SetCommentHiddenAsync(int id, SetHiddenDto model)
        {
            var comment = await _commentRepo.GetByIdAsync(id) ?? throw new NotFoundException();
            comment.IsHidden = model.IsHidden;
            await _commentRepo.UpdateAsync(comment);
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _postRepo.GetByIdAsync(id) ?? throw new NotFoundException();
            await _postService.RemovePostAsync(post);
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _commentRepo.GetByIdAsync(id) ?? throw new NotFoundException();
            await _commentService.RemoveAsync(comment);
        }

        public async Task ResolveReportAsync(int id)
        {
            var report = await _reportRepo.GetByIdAsync(id) ?? throw new NotFoundException();
            report.IsResolved = true;
            await _reportRepo.UpdateAsync(report);
        }
    }
}
