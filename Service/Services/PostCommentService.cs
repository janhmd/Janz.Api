using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Community;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class PostCommentService : IPostCommentService
    {
        private readonly IBaseRepository<Post> _postRepo;
        private readonly IBaseRepository<PostComment> _commentRepo;
        private readonly IBaseRepository<PostCommentVote> _voteRepo;
        private readonly IBaseRepository<PostReport> _reportRepo;
        private readonly IBaseRepository<UserBlock> _blockRepo;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly ICommunityAuthorService _authors;
        private readonly IValidator<CreatePostCommentDto> _createValidator;
        private readonly IValidator<PostVoteDto> _voteValidator;
        private readonly IValidator<ReportPostDto> _reportValidator;

        public PostCommentService(IBaseRepository<Post> postRepo,
                                  IBaseRepository<PostComment> commentRepo,
                                  IBaseRepository<PostCommentVote> voteRepo,
                                  IBaseRepository<PostReport> reportRepo,
                                  IBaseRepository<UserBlock> blockRepo,
                                  IFileService fileService,
                                  INotificationService notificationService,
                                  ICommunityAuthorService authors,
                                  IValidator<CreatePostCommentDto> createValidator,
                                  IValidator<PostVoteDto> voteValidator,
                                  IValidator<ReportPostDto> reportValidator)
        {
            _postRepo = postRepo;
            _commentRepo = commentRepo;
            _voteRepo = voteRepo;
            _reportRepo = reportRepo;
            _blockRepo = blockRepo;
            _fileService = fileService;
            _notificationService = notificationService;
            _authors = authors;
            _createValidator = createValidator;
            _voteValidator = voteValidator;
            _reportValidator = reportValidator;
        }

        public async Task<PostCommentDto> AddAsync(string userId, int postId, CreatePostCommentDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var post = await _postRepo.GetByIdAsync(postId);
            if (post is null || post.IsHidden)
            {
                throw new NotFoundException();
            }

            // Cavab yalnız bir səviyyədir: cavaba cavab üst şərhə bağlanır
            PostComment? parent = null;
            if (model.ParentId.HasValue)
            {
                parent = await _commentRepo.GetByIdAsync(model.ParentId.Value);
                if (parent is null || parent.PostId != postId || parent.IsHidden)
                {
                    throw new NotFoundException();
                }

                if (parent.ParentId.HasValue)
                {
                    parent = await _commentRepo.GetByIdAsync(parent.ParentId.Value);
                }
            }

            var blockedMe = (await _blockRepo.GetAllAsync()).Any(m => m.UserId == post.UserId && m.BlockedId == userId);
            if (blockedMe)
            {
                throw new BadRequestException("You can't comment on this post.");
            }

            string? imagePath = null;
            if (model.Image is not null)
            {
                imagePath = await _fileService.UploadImageAsync(model.Image, CommunityRules.ImageFolder);
            }

            PostComment comment;
            try
            {
                comment = new PostComment
                {
                    UserId = userId,
                    PostId = postId,
                    ParentId = parent?.Id,
                    Text = (model.Text ?? string.Empty).Trim(),
                    ImagePath = imagePath
                };
                await _commentRepo.AddAsync(comment);
            }
            catch
            {
                if (imagePath is not null)
                {
                    await _fileService.DeleteFileAsync(imagePath);
                }
                throw;
            }

            var authors = await _authors.GetAsync(new[] { userId });
            var name = authors[userId].Name;
            var link = "/Community#post-" + postId;

            if (parent is not null && parent.UserId != userId)
            {
                await _notificationService.AddAsync(parent.UserId, NotificationKind.Community, "New reply", name + " replied to your comment.", link);
            }

            if (post.UserId != userId && post.UserId != parent?.UserId)
            {
                await _notificationService.AddAsync(post.UserId, NotificationKind.Community, "New comment", name + " commented on your post.", link);
            }

            return new PostCommentDto
            {
                Id = comment.Id,
                PostId = postId,
                ParentId = comment.ParentId,
                Author = authors[userId],
                Text = comment.Text,
                ImagePath = comment.ImagePath,
                CreatedAt = comment.CreatedAt,
                Score = 0,
                MyVote = 0,
                IsMine = true
            };
        }

        // Öz şərhini və cavablarını silir
        public async Task DeleteAsync(string userId, int id)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment is null || comment.UserId != userId)
            {
                throw new NotFoundException();
            }

            await RemoveAsync(comment);
        }

        public async Task<VoteResultDto> VoteAsync(string userId, int id, PostVoteDto model)
        {
            await _voteValidator.EnsureValidAsync(model);

            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment is null || comment.IsHidden)
            {
                throw new NotFoundException();
            }

            var mine = (await _voteRepo.GetAllAsync()).FirstOrDefault(m => m.CommentId == id && m.UserId == userId);
            if (model.Value == 0 && mine is not null)
            {
                await _voteRepo.DeleteAsync((await _voteRepo.GetByIdAsync(mine.Id))!);
            }
            else if (model.Value != 0 && mine is null)
            {
                await _voteRepo.AddAsync(new PostCommentVote { CommentId = id, UserId = userId, Value = model.Value });
            }
            else if (model.Value != 0 && mine is not null && mine.Value != model.Value)
            {
                var tracked = (await _voteRepo.GetByIdAsync(mine.Id))!;
                tracked.Value = model.Value;
                await _voteRepo.UpdateAsync(tracked);
            }

            var votes = (await _voteRepo.GetAllAsync()).Where(m => m.CommentId == id).ToList();
            return new VoteResultDto { Score = votes.Sum(m => m.Value), MyVote = votes.FirstOrDefault(m => m.UserId == userId)?.Value ?? 0 };
        }

        public async Task ReportAsync(string userId, int id, ReportPostDto model)
        {
            await _reportValidator.EnsureValidAsync(model);

            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment is null || comment.IsHidden)
            {
                throw new NotFoundException();
            }

            if (comment.UserId == userId)
            {
                throw new BadRequestException("You can't report your own comment.");
            }

            var exists = (await _reportRepo.GetAllAsync()).Any(m => m.CommentId == id && m.UserId == userId && !m.IsResolved);
            if (!exists)
            {
                await _reportRepo.AddAsync(new PostReport { UserId = userId, CommentId = id, Reason = model.Reason!.Trim() });
            }
        }

        // Şərhi, cavablarını və onlara yazılan şikayətləri silir (bazada bu əlaqələrdə kaskad yoxdur)
        public async Task RemoveAsync(PostComment comment)
        {
            var all = (await _commentRepo.GetAllAsync()).Where(m => m.PostId == comment.PostId).ToList();
            var doomed = all.Where(m => m.Id == comment.Id || m.ParentId == comment.Id).ToList();
            var ids = doomed.Select(m => m.Id).ToHashSet();
            var files = doomed.Where(m => m.ImagePath != null).Select(m => m.ImagePath!).ToList();

            foreach (var report in (await _reportRepo.GetAllAsync()).Where(m => m.CommentId.HasValue && ids.Contains(m.CommentId.Value)).ToList())
            {
                await _reportRepo.DeleteAsync((await _reportRepo.GetByIdAsync(report.Id))!);
            }

            foreach (var reply in doomed.Where(m => m.Id != comment.Id))
            {
                await _commentRepo.DeleteAsync((await _commentRepo.GetByIdAsync(reply.Id))!);
            }

            await _commentRepo.DeleteAsync((await _commentRepo.GetByIdAsync(comment.Id))!);

            foreach (var path in files)
            {
                await _fileService.DeleteFileAsync(path);
            }
        }
    }
}
