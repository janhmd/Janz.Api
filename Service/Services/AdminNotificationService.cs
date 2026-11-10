using Domain.Entities;
using Domain.Enums;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Dashboard;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Admin üçün bildirişlər: gözləyən sifarişlər, cavabsız müştəri mesajları, baxılmamış şikayətlər, yeni rəylər, az qalan stok.
    // Hamısı mövcud cədvəllərdən hesablanır, ona görə "oxundu" vəziyyəti yoxdur: problem həll olunanda bildiriş özü itir
    public class AdminNotificationService : IAdminNotificationService
    {
        private const int LowStockLimit = 5;          // DashboardService ilə eyni hədd
        private const int ReviewDays = 7;             // bu qədər gün əvvələ qədər olan rəylər yeni sayılır
        private const int MaxPerType = 20;

        private readonly IOrderRepository _orderRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<PostReport> _reportRepo;
        private readonly IBaseRepository<ProductReview> _productReviewRepo;
        private readonly IBaseRepository<ShopReview> _shopReviewRepo;
        private readonly IBaseRepository<ContactMessage> _messageRepo;

        public AdminNotificationService(IOrderRepository orderRepo,
                                        IBaseRepository<Product> productRepo,
                                        IBaseRepository<PostReport> reportRepo,
                                        IBaseRepository<ProductReview> productReviewRepo,
                                        IBaseRepository<ShopReview> shopReviewRepo,
                                        IBaseRepository<ContactMessage> messageRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _reportRepo = reportRepo;
            _productReviewRepo = productReviewRepo;
            _shopReviewRepo = shopReviewRepo;
            _messageRepo = messageRepo;
        }

        public async Task<List<AdminNotificationDto>> GetAsync(IReadOnlyCollection<string> types)
        {
            var result = new List<AdminNotificationDto>();

            if (types.Contains("order"))
            {
                var pending = (await _orderRepo.GetAllForAdminAsync(OrderStatus.Pending))
                    .OrderByDescending(m => m.CreatedAt).Take(MaxPerType);
                foreach (var order in pending)
                {
                    result.Add(new AdminNotificationDto
                    {
                        Type = "order",
                        Severity = "warning",
                        Title = $"New order #{order.Id} is waiting",
                        Text = $"{order.DeliveryFullName} · {order.Total:0.00} · {(order.IsPaid ? "paid" : "not paid yet")}",
                        Link = $"/Admin/Orders/Detail/{order.Id}",
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            if (types.Contains("message"))
            {
                var messages = (await _messageRepo.GetAllAsync()).Where(m => !m.IsAnswered)
                    .OrderByDescending(m => m.CreatedAt).Take(MaxPerType);
                foreach (var message in messages)
                {
                    result.Add(new AdminNotificationDto
                    {
                        Type = "message",
                        Severity = message.Topic == "Complaint" ? "danger" : "warning",
                        Title = message.Topic == "Complaint" ? $"New complaint from {message.Name}" : $"New message from {message.Name}",
                        Text = $"{message.Topic} · {Shorten(message.Message)}",
                        Link = $"/Admin/Messages/Detail/{message.Id}",
                        CreatedAt = message.CreatedAt
                    });
                }
            }

            if (types.Contains("report"))
            {
                var reports = (await _reportRepo.GetAllAsync()).Where(m => !m.IsResolved)
                    .OrderByDescending(m => m.CreatedAt).Take(MaxPerType);
                foreach (var report in reports)
                {
                    result.Add(new AdminNotificationDto
                    {
                        Type = "report",
                        Severity = "danger",
                        Title = report.CommentId is null ? "A community post was reported" : "A community comment was reported",
                        Text = string.IsNullOrWhiteSpace(report.Reason) ? "No reason given" : report.Reason,
                        Link = "/Admin/Community?tab=reports",
                        CreatedAt = report.CreatedAt
                    });
                }
            }

            if (types.Contains("review"))
            {
                var since = DateTime.UtcNow.AddDays(-ReviewDays);
                var products = (await _productRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m.Name);

                foreach (var review in (await _productReviewRepo.GetAllAsync())
                    .Where(m => !m.IsHidden && m.CreatedAt >= since).OrderByDescending(m => m.CreatedAt).Take(MaxPerType))
                {
                    var name = products.TryGetValue(review.ProductId, out var n) ? n : "a product";
                    result.Add(new AdminNotificationDto
                    {
                        Type = "review",
                        Severity = review.Rating <= 2 ? "danger" : "info",
                        Title = $"New {review.Rating}-star review on {name}",
                        Text = string.IsNullOrWhiteSpace(review.Text) ? "Rating only" : Shorten(review.Text),
                        Link = "/Admin/Reviews?type=product",
                        CreatedAt = review.CreatedAt
                    });
                }

                foreach (var review in (await _shopReviewRepo.GetAllAsync())
                    .Where(m => !m.IsHidden && m.CreatedAt >= since).OrderByDescending(m => m.CreatedAt).Take(MaxPerType))
                {
                    var low = review.ShopRating <= 2 || review.DeliveryRating <= 2;
                    result.Add(new AdminNotificationDto
                    {
                        Type = "review",
                        Severity = low ? "danger" : "info",
                        Title = $"New shop review for order #{review.OrderId}",
                        Text = $"Shop {review.ShopRating}/5 · delivery {review.DeliveryRating}/5" + (string.IsNullOrWhiteSpace(review.Comment) ? string.Empty : " · " + Shorten(review.Comment)),
                        Link = "/Admin/Reviews?type=shop",
                        CreatedAt = review.CreatedAt
                    });
                }
            }

            if (types.Contains("stock"))
            {
                foreach (var product in (await _productRepo.GetAllAsync()).Where(m => m.Stock < LowStockLimit).OrderBy(m => m.Stock).Take(MaxPerType))
                {
                    result.Add(new AdminNotificationDto
                    {
                        Type = "stock",
                        Severity = product.Stock <= 0 ? "danger" : "warning",
                        Title = product.Stock <= 0 ? $"{product.Name} is out of stock" : $"{product.Name} is running out",
                        Text = product.Stock <= 0 ? "Customers cannot buy it until it is restocked" : $"Only {product.Stock} left",
                        Link = $"/Admin/Products/Edit/{product.Id}"
                    });
                }
            }

            // Ən təcili olanlar yuxarıda, sonra ən yenilər
            return result
                .OrderBy(m => m.Severity == "danger" ? 0 : m.Severity == "warning" ? 1 : 2)
                .ThenByDescending(m => m.CreatedAt ?? DateTime.MinValue)
                .ToList();
        }

        private static string Shorten(string text)
        {
            text = text.Trim();
            return text.Length <= 120 ? text : text[..117] + "...";
        }
    }
}
