using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Articles;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IBaseRepository<Article> _articleRepo;
        private readonly IBaseRepository<ArticleImage> _imageRepo;
        private readonly IBaseRepository<ArticleLike> _likeRepo;
        private readonly IBaseRepository<SavedArticle> _savedRepo;
        private readonly IBaseRepository<ArticleComment> _commentRepo;
        private readonly IBaseRepository<ArticleCommentVote> _voteRepo;
        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<CreateArticleDto> _createValidator;
        private readonly IValidator<UpdateArticleDto> _updateValidator;
        private readonly IValidator<CreateArticleCommentDto> _commentValidator;
        private readonly IValidator<ArticleVoteDto> _voteValidator;

        public ArticleService(IBaseRepository<Article> articleRepo,
                              IBaseRepository<ArticleImage> imageRepo,
                              IBaseRepository<ArticleLike> likeRepo,
                              IBaseRepository<SavedArticle> savedRepo,
                              IBaseRepository<ArticleComment> commentRepo,
                              IBaseRepository<ArticleCommentVote> voteRepo,
                              IFileService fileService,
                              UserManager<AppUser> userManager,
                              IValidator<CreateArticleDto> createValidator,
                              IValidator<UpdateArticleDto> updateValidator,
                              IValidator<CreateArticleCommentDto> commentValidator,
                              IValidator<ArticleVoteDto> voteValidator)
        {
            _articleRepo = articleRepo;
            _imageRepo = imageRepo;
            _likeRepo = likeRepo;
            _savedRepo = savedRepo;
            _commentRepo = commentRepo;
            _voteRepo = voteRepo;
            _fileService = fileService;
            _userManager = userManager;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _commentValidator = commentValidator;
            _voteValidator = voteValidator;
        }

        public async Task<IEnumerable<ArticleCardDto>> GetPublishedAsync(string? topic, string? tag, string? sort, string? userId)
        {
            var articles = (await _articleRepo.GetAllAsync()).Where(m => m.IsPublished);

            if (!string.IsNullOrWhiteSpace(topic))
            {
                articles = articles.Where(m => string.Equals(m.Topic, topic.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                articles = articles.Where(m => SplitTags(m.Tags).Contains(tag.Trim(), StringComparer.OrdinalIgnoreCase));
            }

            var cards = await ToCardsAsync(articles.ToList(), userId);

            return sort switch
            {
                "old" => cards.OrderBy(m => m.PublishedAt).ToList(),
                "read" => cards.OrderByDescending(m => m.Views).ToList(),
                "popular" => cards.OrderByDescending(m => m.Likes).ThenByDescending(m => m.Views).ToList(),
                _ => cards.OrderByDescending(m => m.PublishedAt).ToList()
            };
        }

        public async Task<ArticleDto> GetBySlugAsync(string slug, string? userId, bool countView = false)
        {
            var found = (await _articleRepo.GetAllAsync()).FirstOrDefault(m => m.Slug == slug && m.IsPublished);
            if (found is null)
            {
                throw new NotFoundException();
            }

            // Baxış yalnız MVC tərəfi (hər oxucu üçün bir dəfə) istəyəndə sayılır; yeniləmə və botlar sayı şişirtmir
            var article = (await _articleRepo.GetByIdAsync(found.Id))!;
            if (countView)
            {
                article.Views += 1;
                await _articleRepo.UpdateAsync(article);
            }

            var dto = new ArticleDto();
            var card = (await ToCardsAsync(new List<Article> { article }, userId)).First();
            CopyCard(card, dto);
            dto.Body = article.Body;
            dto.Images = await ImagesOfAsync(article.Id);
            dto.AuthorName = article.AuthorName;
            dto.AuthorRole = article.AuthorRole;
            dto.AuthorAvatar = await AuthorAvatarAsync(article.AuthorName);
            return dto;
        }

        // Müəllif adı mətn kimi yazılır; eyni ad və soyadlı hesabın profil şəkli varsa o göstərilir
        private async Task<string?> AuthorAvatarAsync(string? authorName)
        {
            var name = authorName?.Trim();
            if (string.IsNullOrEmpty(name)) return null;

            return await _userManager.Users
                .Where(u => u.AvatarPath != null && u.AvatarPath != "" && (u.Name + " " + u.Surname) == name)
                .Select(u => u.AvatarPath)
                .FirstOrDefaultAsync();
        }

        public async Task<int> LikeAsync(int id, string userId, bool like)
        {
            await EnsurePublishedAsync(id);

            var likes = (await _likeRepo.GetAllAsync()).Where(m => m.ArticleId == id).ToList();
            var mine = likes.FirstOrDefault(m => m.UserId == userId);

            if (like && mine is null)
            {
                await _likeRepo.AddAsync(new ArticleLike { ArticleId = id, UserId = userId });
                return likes.Count + 1;
            }

            if (!like && mine is not null)
            {
                await _likeRepo.DeleteAsync((await _likeRepo.GetByIdAsync(mine.Id))!);
                return likes.Count - 1;
            }

            return likes.Count;
        }

        public async Task SaveAsync(int id, string userId, bool save)
        {
            await EnsurePublishedAsync(id);

            var mine = (await _savedRepo.GetAllAsync()).FirstOrDefault(m => m.ArticleId == id && m.UserId == userId);
            if (save && mine is null)
            {
                await _savedRepo.AddAsync(new SavedArticle { ArticleId = id, UserId = userId });
            }
            else if (!save && mine is not null)
            {
                await _savedRepo.DeleteAsync((await _savedRepo.GetByIdAsync(mine.Id))!);
            }
        }

        public async Task<IEnumerable<ArticleCardDto>> GetSavedAsync(string userId)
        {
            // Son saxlanan əvvəldə
            var saved = (await _savedRepo.GetAllAsync()).Where(m => m.UserId == userId).Select(m => m.ArticleId).ToList();
            var articles = (await _articleRepo.GetAllAsync()).Where(m => m.IsPublished && saved.Contains(m.Id)).ToList();
            var cards = await ToCardsAsync(articles, userId);
            return saved.Select(id => cards.FirstOrDefault(m => m.Id == id)).Where(m => m is not null).Select(m => m!).ToList();
        }

        public async Task<IEnumerable<ArticleCommentDto>> GetCommentsAsync(int id, string? userId)
        {
            await EnsurePublishedAsync(id);

            var comments = (await _commentRepo.GetAllAsync()).Where(m => m.ArticleId == id).OrderBy(m => m.CreatedAt).ToList();
            var commentIds = comments.Select(m => m.Id).ToHashSet();
            var votes = (await _voteRepo.GetAllAsync()).Where(m => commentIds.Contains(m.CommentId)).ToList();

            var result = new List<ArticleCommentDto>();
            foreach (var comment in comments)
            {
                result.Add(await ToCommentDtoAsync(comment, votes.Where(m => m.CommentId == comment.Id).ToList(), userId));
            }

            return result;
        }

        public async Task<ArticleCommentDto> AddCommentAsync(int id, string userId, CreateArticleCommentDto model)
        {
            await _commentValidator.EnsureValidAsync(model);
            await EnsurePublishedAsync(id);

            var comment = new ArticleComment
            {
                ArticleId = id,
                UserId = userId,
                Text = model.Text!.Trim()
            };
            await _commentRepo.AddAsync(comment);

            return await ToCommentDtoAsync(comment, new List<ArticleCommentVote>(), userId);
        }

        public async Task DeleteCommentAsync(int commentId, string userId, bool isStaff)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment is null)
            {
                throw new NotFoundException();
            }

            // Öz şərhini müəllif, istənilən şərhi redaktor silir
            if (comment.UserId != userId && !isStaff)
            {
                throw new NotFoundException();
            }

            await _commentRepo.DeleteAsync(comment);
        }

        public async Task<ArticleCommentDto> VoteAsync(int commentId, string userId, ArticleVoteDto model)
        {
            await _voteValidator.EnsureValidAsync(model);

            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment is null)
            {
                throw new NotFoundException();
            }

            var mine = (await _voteRepo.GetAllAsync()).FirstOrDefault(m => m.CommentId == commentId && m.UserId == userId);
            if (model.Value == 0 && mine is not null)
            {
                await _voteRepo.DeleteAsync((await _voteRepo.GetByIdAsync(mine.Id))!);
            }
            else if (model.Value != 0 && mine is null)
            {
                await _voteRepo.AddAsync(new ArticleCommentVote { CommentId = commentId, UserId = userId, Value = model.Value });
            }
            else if (model.Value != 0 && mine is not null && mine.Value != model.Value)
            {
                var tracked = (await _voteRepo.GetByIdAsync(mine.Id))!;
                tracked.Value = model.Value;
                await _voteRepo.UpdateAsync(tracked);
            }

            var votes = (await _voteRepo.GetAllAsync()).Where(m => m.CommentId == commentId).ToList();
            return await ToCommentDtoAsync(comment, votes, userId);
        }

        public async Task<IEnumerable<ArticleAdminDto>> GetAllAsync()
        {
            var articles = (await _articleRepo.GetAllAsync()).ToList();
            var cards = await ToCardsAsync(articles, null);

            return articles.Select(article =>
            {
                var dto = new ArticleAdminDto();
                CopyCard(cards.First(m => m.Id == article.Id), dto);
                FillAdmin(dto, article);
                return dto;
            }).ToList();
        }

        public async Task<ArticleAdminDto> GetByIdAsync(int id)
        {
            var article = await _articleRepo.GetByIdAsync(id);
            if (article is null)
            {
                throw new NotFoundException();
            }

            var dto = new ArticleAdminDto();
            CopyCard((await ToCardsAsync(new List<Article> { article }, null)).First(), dto);
            FillAdmin(dto, article);
            dto.Body = article.Body;
            dto.Images = await ImagesOfAsync(article.Id);
            return dto;
        }

        public async Task<ArticleAdminDto> CreateAsync(CreateArticleDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var slug = await MakeUniqueSlugAsync(model.Slug, model.Title!, null);

            // Əvvəl şəkillər yüklənir: yanlış fayl olsa bazaya heç nə yazılmır
            var uploaded = new List<string>();
            try
            {
                var cover = await _fileService.UploadImageAsync(model.Cover!, ArticleRules.ImageFolder);
                uploaded.Add(cover);
                foreach (var file in model.Images)
                {
                    uploaded.Add(await _fileService.UploadImageAsync(file, ArticleRules.ImageFolder));
                }

                var article = new Article
                {
                    Title = model.Title!.Trim(),
                    Slug = slug,
                    Topic = model.Topic!,
                    Lead = model.Lead!.Trim(),
                    Body = model.Body!.Trim(),
                    CoverPath = cover,
                    Tags = JoinTags(model.Tags),
                    AuthorName = Clean(model.AuthorName),
                    AuthorRole = Clean(model.AuthorRole),
                    IsLearning = model.IsLearning,
                    Series = Clean(model.Series),
                    IsPublished = model.IsPublished,
                    PublishedAt = model.IsPublished ? (model.PublishedAt?.ToUniversalTime() ?? DateTime.UtcNow) : null
                };
                await _articleRepo.AddAsync(article);

                var order = 1;
                foreach (var path in uploaded.Skip(1))
                {
                    await _imageRepo.AddAsync(new ArticleImage { ArticleId = article.Id, ImagePath = path, Order = order++ });
                }

                return await GetByIdAsync(article.Id);
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

        public async Task UpdateAsync(int id, UpdateArticleDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var article = await _articleRepo.GetByIdAsync(id);
            if (article is null)
            {
                throw new NotFoundException();
            }

            article.Slug = await MakeUniqueSlugAsync(model.Slug, model.Title!, id);

            var oldCover = article.CoverPath;
            if (model.Cover is not null)
            {
                article.CoverPath = await _fileService.UploadImageAsync(model.Cover, ArticleRules.ImageFolder);
            }

            // İlk dəfə dərc olunanda tarix yazılır; admin tarix seçibsə o qalır
            if (model.IsPublished && model.PublishedAt.HasValue)
            {
                article.PublishedAt = model.PublishedAt.Value.ToUniversalTime();
            }
            else if (model.IsPublished && article.PublishedAt is null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }

            article.Title = model.Title!.Trim();
            article.Topic = model.Topic!;
            article.Lead = model.Lead!.Trim();
            article.Body = model.Body!.Trim();
            article.Tags = JoinTags(model.Tags);
            article.AuthorName = Clean(model.AuthorName);
            article.AuthorRole = Clean(model.AuthorRole);
            article.IsLearning = model.IsLearning;
            article.Series = Clean(model.Series);
            article.IsPublished = model.IsPublished;
            await _articleRepo.UpdateAsync(article);

            if (model.Cover is not null)
            {
                await _fileService.DeleteFileAsync(oldCover);
            }

            // Silinən şəkillərdən sonra qalanlar 1, 2, 3... nömrələnir, yenilər sona əlavə olunur
            var images = (await _imageRepo.GetAllAsync()).Where(m => m.ArticleId == id).OrderBy(m => m.Order).ToList();
            foreach (var image in images.Where(m => model.RemoveImages.Contains(m.ImagePath)))
            {
                await _imageRepo.DeleteAsync((await _imageRepo.GetByIdAsync(image.Id))!);
                await _fileService.DeleteFileAsync(image.ImagePath);
            }

            var order = 1;
            foreach (var image in images.Where(m => !model.RemoveImages.Contains(m.ImagePath)))
            {
                var tracked = (await _imageRepo.GetByIdAsync(image.Id))!;
                tracked.Order = order++;
                await _imageRepo.UpdateAsync(tracked);
            }

            foreach (var file in model.Images)
            {
                var path = await _fileService.UploadImageAsync(file, ArticleRules.ImageFolder);
                await _imageRepo.AddAsync(new ArticleImage { ArticleId = id, ImagePath = path, Order = order++ });
            }
        }

        public async Task DeleteAsync(int id)
        {
            var article = await _articleRepo.GetByIdAsync(id);
            if (article is null)
            {
                throw new NotFoundException();
            }

            var images = await ImagesOfAsync(id);
            await _articleRepo.DeleteAsync(article);

            await _fileService.DeleteFileAsync(article.CoverPath);
            foreach (var path in images)
            {
                await _fileService.DeleteFileAsync(path);
            }
        }

        private async Task EnsurePublishedAsync(int id)
        {
            var article = (await _articleRepo.GetAllAsync()).FirstOrDefault(m => m.Id == id);
            if (article is null || !article.IsPublished)
            {
                throw new NotFoundException();
            }
        }

        private async Task<List<string>> ImagesOfAsync(int articleId)
        {
            return (await _imageRepo.GetAllAsync())
                .Where(m => m.ArticleId == articleId)
                .OrderBy(m => m.Order)
                .Select(m => m.ImagePath)
                .ToList();
        }

        private async Task<List<ArticleCardDto>> ToCardsAsync(List<Article> articles, string? userId)
        {
            var ids = articles.Select(m => m.Id).ToHashSet();
            var likes = (await _likeRepo.GetAllAsync()).Where(m => ids.Contains(m.ArticleId)).ToList();
            var comments = (await _commentRepo.GetAllAsync()).Where(m => ids.Contains(m.ArticleId)).ToList();
            var saved = userId is null
                ? new HashSet<int>()
                : (await _savedRepo.GetAllAsync()).Where(m => m.UserId == userId).Select(m => m.ArticleId).ToHashSet();

            return articles.Select(m => new ArticleCardDto
            {
                Id = m.Id,
                Title = m.Title,
                Slug = m.Slug,
                Topic = m.Topic,
                Tags = SplitTags(m.Tags),
                Lead = m.Lead,
                CoverPath = m.CoverPath,
                IsLearning = m.IsLearning,
                Series = m.Series,
                PublishedAt = m.PublishedAt ?? m.CreatedAt,
                Likes = likes.Count(l => l.ArticleId == m.Id),
                Comments = comments.Count(c => c.ArticleId == m.Id),
                Views = m.Views,
                Minutes = MinutesOf(m.Body),
                IsLiked = userId is not null && likes.Any(l => l.ArticleId == m.Id && l.UserId == userId),
                IsSaved = saved.Contains(m.Id)
            }).ToList();
        }

        private async Task<ArticleCommentDto> ToCommentDtoAsync(ArticleComment comment, List<ArticleCommentVote> votes, string? userId)
        {
            var user = await _userManager.FindByIdAsync(comment.UserId);
            var name = user is null ? "Deleted user" : string.Join(" ", new[] { user.Name, user.Surname }.Where(m => !string.IsNullOrWhiteSpace(m)));

            return new ArticleCommentDto
            {
                Id = comment.Id,
                AuthorName = string.IsNullOrWhiteSpace(name) ? "Coffee lover" : name,
                AuthorAvatar = user?.AvatarPath,
                StaffRole = user is null ? null : CommunityAuthorService.StaffRoleOf(await _userManager.GetRolesAsync(user)),
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                Likes = votes.Count(m => m.Value > 0),
                Dislikes = votes.Count(m => m.Value < 0),
                IsMine = userId is not null && comment.UserId == userId,
                MyVote = userId is null ? 0 : votes.FirstOrDefault(m => m.UserId == userId)?.Value ?? 0
            };
        }

        private async Task<string> MakeUniqueSlugAsync(string? slug, string title, int? currentId)
        {
            var result = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Make(title) : slug.Trim();
            if (string.IsNullOrEmpty(result))
            {
                throw new BadRequestException("Write a slug with latin letters.");
            }

            var taken = (await _articleRepo.GetAllAsync()).Any(m => m.Id != currentId && m.Slug == result);
            if (taken)
            {
                throw new BadRequestException("This slug is already used by another article.");
            }

            return result;
        }

        private static void CopyCard(ArticleCardDto from, ArticleCardDto to)
        {
            to.Id = from.Id;
            to.Title = from.Title;
            to.Slug = from.Slug;
            to.Topic = from.Topic;
            to.Tags = from.Tags;
            to.Lead = from.Lead;
            to.CoverPath = from.CoverPath;
            to.IsLearning = from.IsLearning;
            to.Series = from.Series;
            to.PublishedAt = from.PublishedAt;
            to.Likes = from.Likes;
            to.Comments = from.Comments;
            to.Views = from.Views;
            to.Minutes = from.Minutes;
            to.IsLiked = from.IsLiked;
            to.IsSaved = from.IsSaved;
        }

        private static void FillAdmin(ArticleAdminDto dto, Article article)
        {
            dto.IsPublished = article.IsPublished;
            dto.CreatedAt = article.CreatedAt;
            dto.AuthorName = article.AuthorName;
            dto.AuthorRole = article.AuthorRole;
        }

        // Oxuma vaxtı: dəqiqədə 200 söz, ən azı 1 dəqiqə
        private static int MinutesOf(string body)
        {
            var words = body.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(words / (double)ArticleRules.WordsPerMinute));
        }

        private static List<string> SplitTags(string? tags)
        {
            return string.IsNullOrWhiteSpace(tags)
                ? new List<string>()
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        private static string? JoinTags(List<string> tags)
        {
            var clean = tags.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();
            return clean.Count == 0 ? null : string.Join(",", clean);
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
