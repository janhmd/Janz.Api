using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Community;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CommunityProfileService : ICommunityProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<UserFollow> _followRepo;
        private readonly IBaseRepository<UserBlock> _blockRepo;
        private readonly IBaseRepository<Post> _postRepo;
        private readonly IBaseRepository<PostVote> _voteRepo;
        private readonly IBaseRepository<PostComment> _commentRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IBaseRepository<OrderItem> _orderItemRepo;
        private readonly IBaseRepository<ProductReview> _reviewRepo;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly ICommunityAuthorService _authors;
        private readonly IValidator<UpdateCommunityProfileDto> _updateValidator;

        public CommunityProfileService(UserManager<AppUser> userManager,
                                       IBaseRepository<UserFollow> followRepo,
                                       IBaseRepository<UserBlock> blockRepo,
                                       IBaseRepository<Post> postRepo,
                                       IBaseRepository<PostVote> voteRepo,
                                       IBaseRepository<PostComment> commentRepo,
                                       IBaseRepository<Order> orderRepo,
                                       IBaseRepository<OrderItem> orderItemRepo,
                                       IBaseRepository<ProductReview> reviewRepo,
                                       IPostService postService,
                                       INotificationService notificationService,
                                       ICommunityAuthorService authors,
                                       IValidator<UpdateCommunityProfileDto> updateValidator)
        {
            _userManager = userManager;
            _followRepo = followRepo;
            _blockRepo = blockRepo;
            _postRepo = postRepo;
            _voteRepo = voteRepo;
            _commentRepo = commentRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _reviewRepo = reviewRepo;
            _postService = postService;
            _notificationService = notificationService;
            _authors = authors;
            _updateValidator = updateValidator;
        }

        public async Task<CommunityProfileDto> GetAsync(string key, string? viewerId)
        {
            var user = await FindAsync(key);
            return await BuildAsync(user, viewerId);
        }

        public async Task<CommunityProfileDto> UpdateMineAsync(string userId, UpdateCommunityProfileDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException();
            user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
            user.CookMethods = model.CookMethods.Count == 0 ? null : string.Join(",", model.CookMethods.Select(m => m.Trim().Replace(",", " ")).Distinct());
            await _userManager.UpdateAsync(user);

            return await BuildAsync(user, userId);
        }

        public async Task FollowAsync(string userId, string targetId)
        {
            if (userId == targetId)
            {
                throw new BadRequestException("You can't follow yourself.");
            }

            _ = await _userManager.FindByIdAsync(targetId) ?? throw new NotFoundException();

            var blocks = (await _blockRepo.GetAllAsync()).Any(m => (m.UserId == userId && m.BlockedId == targetId) || (m.UserId == targetId && m.BlockedId == userId));
            if (blocks)
            {
                throw new BadRequestException("You can't follow this person.");
            }

            var exists = (await _followRepo.GetAllAsync()).Any(m => m.FollowerId == userId && m.FollowedId == targetId);
            if (exists)
            {
                return;
            }

            await _followRepo.AddAsync(new UserFollow { FollowerId = userId, FollowedId = targetId });

            var name = (await _authors.GetAsync(new[] { userId }))[userId].Name;
            await _notificationService.AddAsync(targetId, NotificationKind.Subscription, "New subscriber", name + " subscribed to you.", "/Community/Profile?id=" + userId);
        }

        public async Task UnfollowAsync(string userId, string targetId)
        {
            var follow = (await _followRepo.GetAllAsync()).FirstOrDefault(m => m.FollowerId == userId && m.FollowedId == targetId);
            if (follow is not null)
            {
                await _followRepo.DeleteAsync((await _followRepo.GetByIdAsync(follow.Id))!);
            }
        }

        public async Task<List<CommunityPersonDto>> GetFollowersAsync(string key, string? viewerId)
        {
            var user = await FindAsync(key);
            var ids = (await _followRepo.GetAllAsync()).Where(m => m.FollowedId == user.Id).Select(m => m.FollowerId).ToList();
            return await PeopleAsync(ids, viewerId);
        }

        public async Task<List<CommunityPersonDto>> GetFollowingAsync(string key, string? viewerId)
        {
            var user = await FindAsync(key);
            var ids = (await _followRepo.GetAllAsync()).Where(m => m.FollowerId == user.Id).Select(m => m.FollowedId).ToList();
            return await PeopleAsync(ids, viewerId);
        }

        // Blok: qarşılıqlı abunəlik silinir
        public async Task BlockAsync(string userId, string targetId)
        {
            if (userId == targetId)
            {
                throw new BadRequestException("You can't block yourself.");
            }

            _ = await _userManager.FindByIdAsync(targetId) ?? throw new NotFoundException();

            var exists = (await _blockRepo.GetAllAsync()).Any(m => m.UserId == userId && m.BlockedId == targetId);
            if (!exists)
            {
                await _blockRepo.AddAsync(new UserBlock { UserId = userId, BlockedId = targetId });
            }

            var follows = (await _followRepo.GetAllAsync())
                .Where(m => (m.FollowerId == userId && m.FollowedId == targetId) || (m.FollowerId == targetId && m.FollowedId == userId))
                .ToList();
            foreach (var follow in follows)
            {
                await _followRepo.DeleteAsync((await _followRepo.GetByIdAsync(follow.Id))!);
            }
        }

        public async Task UnblockAsync(string userId, string targetId)
        {
            var block = (await _blockRepo.GetAllAsync()).FirstOrDefault(m => m.UserId == userId && m.BlockedId == targetId);
            if (block is not null)
            {
                await _blockRepo.DeleteAsync((await _blockRepo.GetByIdAsync(block.Id))!);
            }
        }

        public async Task<List<CommunityPersonDto>> GetBlockedAsync(string userId)
        {
            var ids = (await _blockRepo.GetAllAsync()).Where(m => m.UserId == userId).Select(m => m.BlockedId).ToList();
            return await PeopleAsync(ids, userId);
        }

        private async Task<AppUser> FindAsync(string key)
        {
            var user = await _userManager.FindByIdAsync(key);
            if (user is null)
            {
                user = _userManager.Users.FirstOrDefault(m => m.Nickname == key);
            }

            return user ?? throw new NotFoundException();
        }

        private async Task<List<CommunityPersonDto>> PeopleAsync(List<string> ids, string? viewerId)
        {
            if (ids.Count == 0)
            {
                return new List<CommunityPersonDto>();
            }

            var authors = await _authors.GetAsync(ids);
            var following = viewerId is null ? new HashSet<string>() : (await _followRepo.GetAllAsync()).Where(m => m.FollowerId == viewerId).Select(m => m.FollowedId).ToHashSet();

            return ids.Select(id => new CommunityPersonDto { Author = authors[id], IsFollowing = following.Contains(id), IsMe = id == viewerId }).ToList();
        }

        private async Task<CommunityProfileDto> BuildAsync(AppUser user, string? viewerId)
        {
            var authors = await _authors.GetAsync(new[] { user.Id });
            var follows = (await _followRepo.GetAllAsync()).ToList();
            var isBlocked = viewerId is not null && (await _blockRepo.GetAllAsync()).Any(m => m.UserId == viewerId && m.BlockedId == user.Id);

            var posts = isBlocked ? new List<PostDto>() : await _postService.GetByUserAsync(user.Id, viewerId);

            var postIds = (await _postRepo.GetAllAsync()).Where(m => m.UserId == user.Id).Select(m => m.Id).ToHashSet();
            var likes = (await _voteRepo.GetAllAsync()).Count(m => postIds.Contains(m.PostId) && m.Value > 0);

            var delivered = (await _orderRepo.GetAllAsync()).Where(m => m.UserId == user.Id && m.Status == OrderStatus.Delivered).ToList();
            var deliveredIds = delivered.Select(m => m.Id).ToHashSet();
            var tried = (await _orderItemRepo.GetAllAsync()).Where(m => deliveredIds.Contains(m.OrderId)).Select(m => m.ProductId).Distinct().Count();
            var reviews = (await _reviewRepo.GetAllAsync()).Where(m => m.UserId == user.Id && !m.IsHidden).ToList();

            return new CommunityProfileDto
            {
                Author = authors[user.Id],
                Bio = user.Bio,
                CookMethods = string.IsNullOrWhiteSpace(user.CookMethods) ? new List<string>() : user.CookMethods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Followers = follows.Count(m => m.FollowedId == user.Id),
                Following = follows.Count(m => m.FollowerId == user.Id),
                IsMe = user.Id == viewerId,
                IsFollowing = viewerId is not null && follows.Any(m => m.FollowerId == viewerId && m.FollowedId == user.Id),
                IsBlocked = isBlocked,
                Stats = new CommunityStatsDto
                {
                    MemberSince = user.CreatedAt,
                    Orders = delivered.Count,
                    TriedCoffees = tried,
                    AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(m => m.Rating), 1),
                    Posts = postIds.Count,
                    Comments = (await _commentRepo.GetAllAsync()).Count(m => m.UserId == user.Id),
                    LikesReceived = likes
                },
                Posts = posts
            };
        }
    }
}
