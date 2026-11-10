using Domain.Entities;
using Domain.Enums;
using Janz.Tests.Fakes;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class AdminNotificationServiceTests
    {
        private static readonly string[] AllTypes = { "order", "message", "report", "review", "stock" };

        private readonly FakeOrderRepository _orders = new();
        private readonly FakeRepository<Product> _products = new();
        private readonly FakeRepository<PostReport> _reports = new();
        private readonly FakeRepository<ProductReview> _productReviews = new();
        private readonly FakeRepository<ShopReview> _shopReviews = new();
        private readonly FakeRepository<ContactMessage> _messages = new();

        private AdminNotificationService Create() => new(_orders, _products, _reports, _productReviews, _shopReviews, _messages);

        [Fact]
        public async Task Get_ReturnsNothing_WhenNoTypesAllowed()
        {
            _orders.Rows.Add(new Order { Id = 1, Status = OrderStatus.Pending });
            _products.Rows.Add(new Product { Id = 1, Name = "Ethiopia", Stock = 0 });

            var result = await Create().GetAsync(Array.Empty<string>());

            Assert.Empty(result);
        }

        [Fact]
        public async Task Get_ListsOnlyPendingOrders()
        {
            _orders.Rows.Add(new Order { Id = 1, Status = OrderStatus.Pending, DeliveryFullName = "Aysel", Total = 12.5m });
            _orders.Rows.Add(new Order { Id = 2, Status = OrderStatus.Delivered, DeliveryFullName = "Kamran" });
            _orders.Rows.Add(new Order { Id = 3, Status = OrderStatus.Cancelled, DeliveryFullName = "Leyla" });

            var result = await Create().GetAsync(AllTypes);

            var note = Assert.Single(result);
            Assert.Equal("order", note.Type);
            Assert.Equal("/Admin/Orders/Detail/1", note.Link);
            Assert.Contains("Aysel", note.Text);
        }

        [Fact]
        public async Task Get_SkipsResolvedReports()
        {
            _reports.Rows.Add(new PostReport { Id = 1, Reason = "Spam", IsResolved = false });
            _reports.Rows.Add(new PostReport { Id = 2, Reason = "Old", IsResolved = true });

            var result = await Create().GetAsync(AllTypes);

            var note = Assert.Single(result);
            Assert.Equal("report", note.Type);
            Assert.Equal("Spam", note.Text);
        }

        [Fact]
        public async Task Get_ShowsRecentVisibleReviews_AndFlagsLowRatings()
        {
            _products.Rows.Add(new Product { Id = 1, Name = "Ethiopia", Stock = 50 });
            _productReviews.Rows.Add(new ProductReview { Id = 1, ProductId = 1, Rating = 1, Text = "Bitter", CreatedAt = DateTime.UtcNow.AddDays(-1) });
            _productReviews.Rows.Add(new ProductReview { Id = 2, ProductId = 1, Rating = 5, CreatedAt = DateTime.UtcNow.AddDays(-30) });
            _productReviews.Rows.Add(new ProductReview { Id = 3, ProductId = 1, Rating = 5, IsHidden = true, CreatedAt = DateTime.UtcNow });

            var result = await Create().GetAsync(AllTypes);

            var note = Assert.Single(result);
            Assert.Equal("danger", note.Severity);
            Assert.Contains("Ethiopia", note.Title);
        }

        [Fact]
        public async Task Get_FlagsShopReview_WhenDeliveryIsRatedLow()
        {
            _shopReviews.Rows.Add(new ShopReview { Id = 1, OrderId = 9, ShopRating = 5, DeliveryRating = 1, CreatedAt = DateTime.UtcNow });

            var result = await Create().GetAsync(AllTypes);

            var note = Assert.Single(result);
            Assert.Equal("danger", note.Severity);
            Assert.Contains("#9", note.Title);
        }

        [Fact]
        public async Task Get_WarnsAboutLowStock_AndMarksOutOfStockAsDanger()
        {
            _products.Rows.Add(new Product { Id = 1, Name = "Plenty", Stock = 40 });
            _products.Rows.Add(new Product { Id = 2, Name = "Almost gone", Stock = 3 });
            _products.Rows.Add(new Product { Id = 3, Name = "Sold out", Stock = 0 });

            var result = await Create().GetAsync(AllTypes);

            Assert.Equal(2, result.Count);
            Assert.Equal("Sold out", result[0].Title.Replace(" is out of stock", string.Empty));
            Assert.Equal("danger", result[0].Severity);
            Assert.Equal("warning", result[1].Severity);
        }

        [Fact]
        public async Task Get_OnlyReturnsAllowedTypes()
        {
            _orders.Rows.Add(new Order { Id = 1, Status = OrderStatus.Pending });
            _products.Rows.Add(new Product { Id = 1, Name = "Sold out", Stock = 0 });

            var result = await Create().GetAsync(new[] { "stock" });

            var note = Assert.Single(result);
            Assert.Equal("stock", note.Type);
        }

        [Fact]
        public async Task Get_PutsUrgentNotesFirst()
        {
            _orders.Rows.Add(new Order { Id = 1, Status = OrderStatus.Pending });
            _products.Rows.Add(new Product { Id = 1, Name = "Sold out", Stock = 0 });
            _shopReviews.Rows.Add(new ShopReview { Id = 1, OrderId = 1, ShopRating = 5, DeliveryRating = 5, CreatedAt = DateTime.UtcNow });

            var result = await Create().GetAsync(AllTypes);

            Assert.Equal(new[] { "danger", "warning", "info" }, result.Select(m => m.Severity).ToArray());
        }

        [Fact]
        public async Task Get_ListsOnlyUnansweredMessages_ComplaintsAreDanger()
        {
            _messages.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Topic = "Complaint", Message = "The parcel arrived damaged", IsAnswered = false });
            _messages.Rows.Add(new ContactMessage { Id = 2, Name = "Kamran", Topic = "Order", Message = "When will it arrive?", IsAnswered = false });
            _messages.Rows.Add(new ContactMessage { Id = 3, Name = "Leyla", Topic = "Other", Message = "Thanks for the help", IsAnswered = true });

            var result = await Create().GetAsync(new[] { "message" });

            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.Equal("message", m.Type));
            Assert.Equal("danger", result.First(m => m.Link.EndsWith("/1")).Severity);
            Assert.Equal("warning", result.First(m => m.Link.EndsWith("/2")).Severity);
            Assert.DoesNotContain(result, m => m.Link.EndsWith("/3"));
        }

        [Fact]
        public async Task Get_SkipsMessages_WhenTypeNotAllowed()
        {
            _messages.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Topic = "Order", Message = "Where is my order?" });

            var result = await Create().GetAsync(new[] { "order" });

            Assert.Empty(result);
        }
    }
}
