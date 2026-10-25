using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Reviews;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IBaseRepository<ProductReview> _productReviewRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IBaseRepository<OrderItem> _orderItemRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<SaveProductReviewDto> _productReviewValidator;
        private readonly IBaseRepository<ShopReview> _shopReviewRepo;
        private readonly IValidator<SaveShopReviewDto> _shopReviewValidator;

        public ReviewService(IBaseRepository<ProductReview> productReviewRepo,
                             IBaseRepository<Product> productRepo,
                             IBaseRepository<Order> orderRepo,
                             IBaseRepository<OrderItem> orderItemRepo,
                             UserManager<AppUser> userManager,
                             IValidator<SaveProductReviewDto> productReviewValidator,
                             IBaseRepository<ShopReview> shopReviewRepo,
                             IValidator<SaveShopReviewDto> shopReviewValidator)
        {
            _productReviewRepo = productReviewRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _userManager = userManager;
            _productReviewValidator = productReviewValidator;
            _shopReviewRepo = shopReviewRepo;
            _shopReviewValidator = shopReviewValidator;
        }

        public async Task<ShopReviewsDto> GetShopReviewsAsync(int skip, int take)
        {
            var visible = (await _shopReviewRepo.GetAllAsync()).Where(m => !m.IsHidden).OrderByDescending(m => m.CreatedAt).ToList();
            var page = visible.Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 50)).ToList();
            var names = await AuthorNamesAsync(page.Select(m => m.UserId));
            var levels = await AuthorLevelsAsync(page.Select(m => m.UserId));

            return new ShopReviewsDto
            {
                Total = visible.Count,
                ShopAverage = visible.Count == 0 ? null : Math.Round((decimal)visible.Average(m => m.ShopRating), 1),
                DeliveryAverage = visible.Count == 0 ? null : Math.Round((decimal)visible.Average(m => m.DeliveryRating), 1),
                ShopCounts = Enumerable.Range(1, 5).Reverse().Select(star => visible.Count(m => m.ShopRating == star)).ToList(),
                DeliveryCounts = Enumerable.Range(1, 5).Reverse().Select(star => visible.Count(m => m.DeliveryRating == star)).ToList(),
                Items = page.Select(m => ToDto(m, names, levels)).ToList()
            };
        }

        public async Task<OrderReviewDto> GetForOrderAsync(int orderId, string userId)
        {
            var order = await GetOwnOrderAsync(orderId, userId);
            var review = (await _shopReviewRepo.GetAllAsync()).FirstOrDefault(m => m.OrderId == orderId);

            return new OrderReviewDto
            {
                CanReview = order.Status == OrderStatus.Delivered,
                Review = review is null ? null : ToDto(review, await AuthorNamesAsync(new[] { userId }), await AuthorLevelsAsync(new[] { userId }))
            };
        }

        public async Task<ShopReviewDto> SaveForOrderAsync(int orderId, string userId, SaveShopReviewDto model)
        {
            await _shopReviewValidator.EnsureValidAsync(model);

            var order = await GetOwnOrderAsync(orderId, userId);
            if (order.Status != OrderStatus.Delivered)
            {
                throw new BadRequestException("You can review an order after it is delivered.");
            }

            var review = (await _shopReviewRepo.GetAllAsync()).FirstOrDefault(m => m.OrderId == orderId);
            var isNew = review is null;
            review ??= new ShopReview { OrderId = orderId, UserId = userId };
            review.ShopRating = model.ShopRating;
            review.ShopText = Clean(model.ShopText);
            review.DeliveryRating = model.DeliveryRating;
            review.DeliveryText = Clean(model.DeliveryText);
            review.Comment = Clean(model.Comment);

            if (isNew)
            {
                await _shopReviewRepo.AddAsync(review);
            }
            else
            {
                review.UpdatedAt = DateTime.UtcNow;
                await _shopReviewRepo.UpdateAsync(review);
            }

            return ToDto(review, await AuthorNamesAsync(new[] { userId }), await AuthorLevelsAsync(new[] { userId }));
        }

        // Başqasının sifarişi tapılmamış kimi qaytarılır
        private async Task<Order> GetOwnOrderAsync(int orderId, string userId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order is null || order.UserId != userId) throw new NotFoundException();
            return order;
        }

        // Çatdırılmış sifariş sayına görə: 1 - Newcomer, 2-4 - Regular, 5+ - Coffee lover
        private async Task<Dictionary<string, string>> AuthorLevelsAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.ToHashSet();
            var counts = (await _orderRepo.GetAllAsync())
                .Where(m => ids.Contains(m.UserId) && m.Status == OrderStatus.Delivered)
                .GroupBy(m => m.UserId)
                .ToDictionary(m => m.Key, m => m.Count());

            return ids.ToDictionary(id => id, id =>
            {
                var count = counts.TryGetValue(id, out var value) ? value : 0;
                return count >= 5 ? "Coffee lover" : count >= 2 ? "Regular" : "Newcomer";
            });
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ShopReviewDto ToDto(ShopReview review, Dictionary<string, string> names, Dictionary<string, string> levels)
        {
            return new ShopReviewDto
            {
                Id = review.Id,
                OrderId = review.OrderId,
                AuthorName = names.TryGetValue(review.UserId, out var name) && name.Length > 0 ? name : "Customer",
                AuthorLevel = levels.TryGetValue(review.UserId, out var level) ? level : "Newcomer",
                ShopRating = review.ShopRating,
                ShopText = review.ShopText,
                DeliveryRating = review.DeliveryRating,
                DeliveryText = review.DeliveryText,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<ProductReviewsDto> GetForProductAsync(int productId, string? userId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null || !product.IsVisible) throw new NotFoundException();

            var reviews = (await _productReviewRepo.GetAllAsync()).Where(m => m.ProductId == productId).ToList();
            var visible = reviews.Where(m => !m.IsHidden).OrderByDescending(m => m.CreatedAt).ToList();
            var names = await AuthorNamesAsync(visible.Select(m => m.UserId));

            var result = new ProductReviewsDto
            {
                Count = visible.Count,
                Average = visible.Count == 0 ? null : Math.Round((decimal)visible.Average(m => m.Rating), 1),
                Items = visible.Select(m => ToDto(m, names, userId)).ToList()
            };

            if (!string.IsNullOrEmpty(userId))
            {
                var mine = reviews.FirstOrDefault(m => m.UserId == userId);
                result.MyReview = mine is null ? null : ToDto(mine, await AuthorNamesAsync(new[] { userId }), userId);
                result.CanReview = await HasReceivedAsync(productId, userId);
            }

            return result;
        }

        // Rəy yoxdursa yaranır, varsa dəyişir. Gizlədilmiş rəy dəyişəndə gizli qalır
        public async Task<ProductReviewDto> SaveForProductAsync(int productId, string userId, SaveProductReviewDto model)
        {
            await _productReviewValidator.EnsureValidAsync(model);

            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null || !product.IsVisible) throw new NotFoundException();

            if (!await HasReceivedAsync(productId, userId))
            {
                throw new BadRequestException("You can review a product after your order with it is delivered.");
            }

            var text = string.IsNullOrWhiteSpace(model.Text) ? null : model.Text.Trim();
            var review = (await _productReviewRepo.GetAllAsync()).FirstOrDefault(m => m.ProductId == productId && m.UserId == userId);
            if (review is null)
            {
                review = new ProductReview { ProductId = productId, UserId = userId, Rating = model.Rating, Text = text };
                await _productReviewRepo.AddAsync(review);
            }
            else
            {
                review.Rating = model.Rating;
                review.Text = text;
                review.UpdatedAt = DateTime.UtcNow;
                await _productReviewRepo.UpdateAsync(review);
            }

            return ToDto(review, await AuthorNamesAsync(new[] { userId }), userId);
        }

        public async Task DeleteForProductAsync(int productId, string userId)
        {
            var review = (await _productReviewRepo.GetAllAsync()).FirstOrDefault(m => m.ProductId == productId && m.UserId == userId);
            if (review is null) throw new NotFoundException();

            await _productReviewRepo.DeleteAsync(review);
        }

        // Məhsul istifadəçinin çatdırılmış sifarişlərindən birindədirmi
        private async Task<bool> HasReceivedAsync(int productId, string userId)
        {
            var delivered = (await _orderRepo.GetAllAsync())
                .Where(m => m.UserId == userId && m.Status == OrderStatus.Delivered)
                .Select(m => m.Id)
                .ToHashSet();
            if (delivered.Count == 0) return false;

            return (await _orderItemRepo.GetAllAsync()).Any(m => m.ProductId == productId && delivered.Contains(m.OrderId));
        }

        // Rəydə tam soyad göstərilmir: "Aysel M."
        private async Task<Dictionary<string, string>> AuthorNamesAsync(IEnumerable<string> userIds)
        {
            var names = new Dictionary<string, string>();
            foreach (var id in userIds.Distinct())
            {
                var user = await _userManager.FindByIdAsync(id);
                names[id] = user is null
                    ? "Customer"
                    : (user.Name + (string.IsNullOrWhiteSpace(user.Surname) ? string.Empty : $" {user.Surname[0]}.")).Trim();
            }

            return names;
        }

        private static ProductReviewDto ToDto(ProductReview review, Dictionary<string, string> names, string? userId)
        {
            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                AuthorName = names.TryGetValue(review.UserId, out var name) && name.Length > 0 ? name : "Customer",
                Rating = review.Rating,
                Text = review.Text,
                CreatedAt = review.CreatedAt,
                IsMine = review.UserId == userId
            };
        }
    }
}
