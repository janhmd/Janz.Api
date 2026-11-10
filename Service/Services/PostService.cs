using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Community;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class PostService : IPostService
    {
        private readonly IBaseRepository<Post> _postRepo;
        private readonly IBaseRepository<PostImage> _imageRepo;
        private readonly IBaseRepository<PostVote> _voteRepo;
        private readonly IBaseRepository<SavedPost> _savedRepo;
        private readonly IBaseRepository<PostComment> _commentRepo;
        private readonly IBaseRepository<PostCommentVote> _commentVoteRepo;
        private readonly IBaseRepository<PostReport> _reportRepo;
        private readonly IBaseRepository<UserFollow> _followRepo;
        private readonly IBaseRepository<UserBlock> _blockRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IFileService _fileService;
        private readonly ICommunityAuthorService _authors;
        private readonly IValidator<CreatePostDto> _createValidator;
        private readonly IValidator<PostVoteDto> _voteValidator;
        private readonly IValidator<ReportPostDto> _reportValidator;

        public PostService(IBaseRepository<Post> postRepo,
                           IBaseRepository<PostImage> imageRepo,
                           IBaseRepository<PostVote> voteRepo,
                           IBaseRepository<SavedPost> savedRepo,
                           IBaseRepository<PostComment> commentRepo,
                           IBaseRepository<PostCommentVote> commentVoteRepo,
                           IBaseRepository<PostReport> reportRepo,
                           IBaseRepository<UserFollow> followRepo,
                           IBaseRepository<UserBlock> blockRepo,
                           IBaseRepository<Product> productRepo,
                           IFileService fileService,
                           ICommunityAuthorService authors,
                           IValidator<CreatePostDto> createValidator,
                           IValidator<PostVoteDto> voteValidator,
                           IValidator<ReportPostDto> reportValidator)
        {
            _postRepo = postRepo;
            _imageRepo = imageRepo;
            _voteRepo = voteRepo;
            _savedRepo = savedRepo;
            _commentRepo = commentRepo;
            _commentVoteRepo = commentVoteRepo;
            _reportRepo = reportRepo;
            _followRepo = followRepo;
            _blockRepo = blockRepo;
            _productRepo = productRepo;
            _fileService = fileService;
            _authors = authors;
            _createValidator = createValidator;
            _voteValidator = voteValidator;
            _reportValidator = reportValidator;
        }

        public async Task<IEnumerable<PostDto>> GetFeedAsync(string? tab, string? tag, string? search, string? scope, string? viewerId)
        {
            var blocked = await BlockedByAsync(viewerId);
            var posts = (await _postRepo.GetAllAsync()).Where(m => !m.IsHidden && !blocked.Contains(m.UserId)).ToList();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                posts = posts.Where(m => SplitTags(m.Tags).Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            var dtos = await BuildAsync(posts, viewerId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var inPosts = scope != "comments";
                var inComments = scope != "posts";
                dtos = dtos.Where(m =>
                        (inPosts && (m.Text.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || m.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase))
                            || m.Author.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                        || (inComments && m.Comments.Any(c => Matches(c, term))))
                    .ToList();
            }

            return string.Equals(tab, "new", StringComparison.OrdinalIgnoreCase)
                ? dtos.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id)
                : dtos.OrderByDescending(m => m.Score).ThenByDescending(m => m.CreatedAt);
        }

        public async Task<List<PostDto>> GetByUserAsync(string authorId, string? viewerId)
        {
            var posts = (await _postRepo.GetAllAsync()).Where(m => m.UserId == authorId && !m.IsHidden).ToList();
            return (await BuildAsync(posts, viewerId)).OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).ToList();
        }

        public async Task<List<PostDto>> GetByProductAsync(int productId, string? viewerId)
        {
            var blocked = await BlockedByAsync(viewerId);
            var posts = (await _postRepo.GetAllAsync())
                .Where(m => m.ProductId == productId && !m.IsHidden && !blocked.Contains(m.UserId))
                .ToList();
            return (await BuildAsync(posts, viewerId)).OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).ToList();
        }

        public async Task<IEnumerable<PostDto>> GetSavedAsync(string viewerId)
        {
            var saved = (await _savedRepo.GetAllAsync()).Where(m => m.UserId == viewerId).Select(m => m.PostId).ToHashSet();
            var blocked = await BlockedByAsync(viewerId);
            var posts = (await _postRepo.GetAllAsync()).Where(m => saved.Contains(m.Id) && !m.IsHidden && !blocked.Contains(m.UserId)).ToList();
            return (await BuildAsync(posts, viewerId)).OrderByDescending(m => m.CreatedAt);
        }

        public async Task<PostDto> CreateAsync(string userId, CreatePostDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            if (model.ProductId.HasValue)
            {
                var product = await _productRepo.GetByIdAsync(model.ProductId.Value);
                if (product is null || !product.IsVisible)
                {
                    throw new BadRequestException("This product is not available.");
                }
            }
            else if (model.Rating.HasValue)
            {
                throw new BadRequestException("Choose a product to rate it.");
            }

            var uploaded = new List<string>();
            try
            {
                foreach (var file in model.Images)
                {
                    uploaded.Add(await _fileService.UploadImageAsync(file, CommunityRules.ImageFolder));
                }

                var post = new Post
                {
                    UserId = userId,
                    Text = model.Text!.Trim(),
                    ProductId = model.ProductId,
                    Rating = model.ProductId.HasValue ? model.Rating : null,
                    Brew = model.ProductId.HasValue ? Clean(model.Brew) : null,
                    Recipe = model.ProductId.HasValue ? Clean(model.Recipe) : null,
                    Dose = model.ProductId.HasValue ? Clean(model.Dose) : null,
                    Water = model.ProductId.HasValue ? Clean(model.Water) : null,
                    Grind = model.ProductId.HasValue ? Clean(model.Grind) : null,
                    Time = model.ProductId.HasValue ? Clean(model.Time) : null,
                    Tags = model.Tags.Count == 0 ? null : string.Join(",", model.Tags.Distinct())
                };
                await _postRepo.AddAsync(post);

                var order = 1;
                foreach (var path in uploaded)
                {
                    await _imageRepo.AddAsync(new PostImage { PostId = post.Id, ImagePath = path, Order = order++ });
                }

                return (await BuildAsync(new[] { post }, userId)).First();
            }
            catch
            {
                foreach (var path in uploaded)
                {
                    await _fileService.DeleteFileAsync(path);
                }
                throw;
            }
        }

        // Yalnız öz postunu silir. Şərhlər, səslər, şəkillər və saxlananlar bazada postla birlikdə silinir
        public async Task DeleteAsync(string userId, int id)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post is null || post.UserId != userId)
            {
                throw new NotFoundException();
            }

            await RemovePostAsync(post);
        }

        public async Task<VoteResultDto> VoteAsync(string userId, int id, PostVoteDto model)
        {
            await _voteValidator.EnsureValidAsync(model);
            await VisiblePostAsync(id);

            var mine = (await _voteRepo.GetAllAsync()).FirstOrDefault(m => m.PostId == id && m.UserId == userId);
            if (model.Value == 0 && mine is not null)
            {
                await _voteRepo.DeleteAsync((await _voteRepo.GetByIdAsync(mine.Id))!);
            }
            else if (model.Value != 0 && mine is null)
            {
                await _voteRepo.AddAsync(new PostVote { PostId = id, UserId = userId, Value = model.Value });
            }
            else if (model.Value != 0 && mine is not null && mine.Value != model.Value)
            {
                var tracked = (await _voteRepo.GetByIdAsync(mine.Id))!;
                tracked.Value = model.Value;
                await _voteRepo.UpdateAsync(tracked);
            }

            var votes = (await _voteRepo.GetAllAsync()).Where(m => m.PostId == id).ToList();
            return new VoteResultDto { Score = votes.Sum(m => m.Value), MyVote = votes.FirstOrDefault(m => m.UserId == userId)?.Value ?? 0 };
        }

        public async Task SaveAsync(string userId, int id, bool on)
        {
            await VisiblePostAsync(id);

            var mine = (await _savedRepo.GetAllAsync()).FirstOrDefault(m => m.PostId == id && m.UserId == userId);
            if (on && mine is null)
            {
                await _savedRepo.AddAsync(new SavedPost { PostId = id, UserId = userId });
            }
            else if (!on && mine is not null)
            {
                await _savedRepo.DeleteAsync((await _savedRepo.GetByIdAsync(mine.Id))!);
            }
        }

        public async Task ReportAsync(string userId, int id, ReportPostDto model)
        {
            await _reportValidator.EnsureValidAsync(model);
            var post = await VisiblePostAsync(id);
            if (post.UserId == userId)
            {
                throw new BadRequestException("You can't report your own post.");
            }

            var exists = (await _reportRepo.GetAllAsync()).Any(m => m.PostId == id && m.CommentId == null && m.UserId == userId && !m.IsResolved);
            if (!exists)
            {
                await _reportRepo.AddAsync(new PostReport { UserId = userId, PostId = id, Reason = model.Reason!.Trim() });
            }
        }

        // Postu və ona bağlı hər şeyi silir (admin də bunu çağırır). Şəkil faylları da silinir
        public async Task RemovePostAsync(Post post)
        {
            var comments = (await _commentRepo.GetAllAsync()).Where(m => m.PostId == post.Id).ToList();
            var commentIds = comments.Select(m => m.Id).ToHashSet();
            var images = (await _imageRepo.GetAllAsync()).Where(m => m.PostId == post.Id).Select(m => m.ImagePath).ToList();
            var files = images.Concat(comments.Where(m => m.ImagePath != null).Select(m => m.ImagePath!)).ToList();

            // Şərhə yazılan şikayət şərhdən əvvəl silinir (bazada şərh tərəfində kaskad yoxdur)
            foreach (var report in (await _reportRepo.GetAllAsync()).Where(m => m.CommentId.HasValue && commentIds.Contains(m.CommentId.Value)).ToList())
            {
                await _reportRepo.DeleteAsync((await _reportRepo.GetByIdAsync(report.Id))!);
            }

            await _postRepo.DeleteAsync((await _postRepo.GetByIdAsync(post.Id))!);

            foreach (var path in files)
            {
                await _fileService.DeleteFileAsync(path);
            }
        }

        private async Task<Post> VisiblePostAsync(int id)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post is null || post.IsHidden)
            {
                throw new NotFoundException();
            }

            return post;
        }

        // Baxanın blokladıqları: onların postları və şərhləri görünmür
        private async Task<HashSet<string>> BlockedByAsync(string? viewerId)
        {
            if (viewerId is null)
            {
                return new HashSet<string>();
            }

            return (await _blockRepo.GetAllAsync()).Where(m => m.UserId == viewerId).Select(m => m.BlockedId).ToHashSet();
        }

        private async Task<List<PostDto>> BuildAsync(IEnumerable<Post> source, string? viewerId)
        {
            var posts = source.ToList();
            var ids = posts.Select(m => m.Id).ToHashSet();
            var blocked = await BlockedByAsync(viewerId);

            var images = (await _imageRepo.GetAllAsync()).Where(m => ids.Contains(m.PostId)).OrderBy(m => m.Order).ToList();
            var votes = (await _voteRepo.GetAllAsync()).Where(m => ids.Contains(m.PostId)).ToList();
            var saved = viewerId is null ? new HashSet<int>() : (await _savedRepo.GetAllAsync()).Where(m => m.UserId == viewerId).Select(m => m.PostId).ToHashSet();
            var comments = (await _commentRepo.GetAllAsync()).Where(m => ids.Contains(m.PostId) && !m.IsHidden && !blocked.Contains(m.UserId)).OrderBy(m => m.CreatedAt).ThenBy(m => m.Id).ToList();
            var commentIds = comments.Select(m => m.Id).ToHashSet();
            var commentVotes = (await _commentVoteRepo.GetAllAsync()).Where(m => commentIds.Contains(m.CommentId)).ToList();
            var following = viewerId is null ? new HashSet<string>() : (await _followRepo.GetAllAsync()).Where(m => m.FollowerId == viewerId).Select(m => m.FollowedId).ToHashSet();
            var productIds = posts.Where(m => m.ProductId.HasValue).Select(m => m.ProductId!.Value).ToHashSet();
            var products = (await _productRepo.GetAllAsync()).Where(m => productIds.Contains(m.Id)).ToDictionary(m => m.Id);
            var authors = await _authors.GetAsync(posts.Select(m => m.UserId).Concat(comments.Select(m => m.UserId)));

            return posts.Select(post =>
            {
                var postVotes = votes.Where(m => m.PostId == post.Id).ToList();
                var postComments = comments.Where(m => m.PostId == post.Id).ToList();
                products.TryGetValue(post.ProductId ?? 0, out var product);

                return new PostDto
                {
                    Id = post.Id,
                    Author = authors[post.UserId],
                    Text = post.Text,
                    Images = images.Where(m => m.PostId == post.Id).Select(m => m.ImagePath).ToList(),
                    Product = product is null ? null : new PostProductDto { Id = product.Id, Name = product.Name, ImagePath = product.ImagePath },
                    Rating = post.Rating,
                    Brew = post.Brew,
                    Recipe = post.Recipe,
                    Dose = post.Dose,
                    Water = post.Water,
                    Grind = post.Grind,
                    Time = post.Time,
                    Tags = SplitTags(post.Tags),
                    CreatedAt = post.CreatedAt,
                    Score = postVotes.Sum(m => m.Value),
                    MyVote = postVotes.FirstOrDefault(m => m.UserId == viewerId)?.Value ?? 0,
                    IsSaved = saved.Contains(post.Id),
                    IsMine = post.UserId == viewerId,
                    IsFollowing = following.Contains(post.UserId),
                    CommentCount = postComments.Count,
                    Comments = CommentTree(postComments, commentVotes, authors, viewerId)
                };
            }).ToList();
        }

        // Üst şərhlər və onların cavabları (cavabın üstündəki şərh görünmürsə cavab da görünmür)
        private static List<PostCommentDto> CommentTree(List<PostComment> comments, List<PostCommentVote> votes, Dictionary<string, AuthorDto> authors, string? viewerId)
        {
            PostCommentDto ToDto(PostComment comment)
            {
                var own = votes.Where(m => m.CommentId == comment.Id).ToList();
                return new PostCommentDto
                {
                    Id = comment.Id,
                    PostId = comment.PostId,
                    ParentId = comment.ParentId,
                    Author = authors[comment.UserId],
                    Text = comment.Text,
                    ImagePath = comment.ImagePath,
                    CreatedAt = comment.CreatedAt,
                    Score = own.Sum(m => m.Value),
                    MyVote = own.FirstOrDefault(m => m.UserId == viewerId)?.Value ?? 0,
                    IsMine = comment.UserId == viewerId
                };
            }

            var top = comments.Where(m => m.ParentId == null).Select(ToDto).ToList();
            foreach (var reply in comments.Where(m => m.ParentId != null))
            {
                top.FirstOrDefault(m => m.Id == reply.ParentId)?.Replies.Add(ToDto(reply));
            }

            return top;
        }

        private static bool Matches(PostCommentDto comment, string term)
        {
            return comment.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || comment.Replies.Any(m => Matches(m, term));
        }

        private static List<string> SplitTags(string? tags)
        {
            return string.IsNullOrWhiteSpace(tags) ? new List<string>() : tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
