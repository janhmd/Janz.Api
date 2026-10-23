using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Dashboard;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class DashboardService : IDashboardService
    {
        private const int LowStockLimit = 5;         // bu saydan az qalan məhsul xəbərdarlıqda görünür
        private const int ListSize = 5;

        private readonly IOrderRepository _orderRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly UserManager<AppUser> _userManager;

        public DashboardService(IOrderRepository orderRepo,
                                IBaseRepository<Product> productRepo,
                                UserManager<AppUser> userManager)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        public async Task<DashboardDto> GetAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var orders = await _orderRepo.GetAllForAdminAsync(null);
            var paid = orders.Where(m => m.Status != OrderStatus.Cancelled).ToList();
            var products = (await _productRepo.GetAllAsync()).ToList();

            // Müştəri: yalnız User rolunda olanlar (işçilər sayılmır)
            var staffIds = new HashSet<string>();
            foreach (var role in Roles.Staff)
            {
                foreach (var staff in await _userManager.GetUsersInRoleAsync(role))
                {
                    staffIds.Add(staff.Id);
                }
            }
            var customers = (await _userManager.Users.AsNoTracking().ToListAsync())
                .Where(m => !staffIds.Contains(m.Id))
                .ToList();

            var dto = new DashboardDto
            {
                TotalRevenue = paid.Sum(m => m.Total),
                RevenueThisMonth = paid.Where(m => m.CreatedAt >= monthStart).Sum(m => m.Total),
                OrderCount = orders.Count,
                OrdersToday = orders.Count(m => m.CreatedAt.Date == now.Date),
                PendingOrders = orders.Count(m => m.Status == OrderStatus.Pending),
                AverageOrder = paid.Count == 0 ? 0 : Math.Round(paid.Average(m => m.Total), 2),
                CustomerCount = customers.Count,
                NewCustomersThisMonth = customers.Count(m => m.CreatedAt >= monthStart),
                ProductCount = products.Count
            };

            foreach (var status in Enum.GetValues<OrderStatus>())
            {
                dto.OrdersByStatus[status.ToString()] = orders.Count(m => m.Status == status);
            }

            // Son 7 gün: satış olmayan gün də 0 ilə göstərilir ki, qrafikdə boşluq olmasın
            for (var i = 6; i >= 0; i--)
            {
                var day = now.Date.AddDays(-i);
                var dayOrders = paid.Where(m => m.CreatedAt.Date == day).ToList();
                dto.SalesLast7Days.Add(new DailySalesDto
                {
                    Date = day,
                    Total = dayOrders.Sum(m => m.Total),
                    Orders = dayOrders.Count
                });
            }

            // Ən çox satılanlar: sifarişdəki ad və şəkil (məhsul sonra dəyişsə də satış tarixçəsi qalır)
            dto.TopProducts = paid
                .SelectMany(m => m.Items)
                .GroupBy(m => m.ProductId)
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key,
                    Name = g.First().ProductName,
                    ImagePath = g.First().ImagePath,
                    Sold = g.Sum(m => m.Quantity),
                    Revenue = g.Sum(m => m.Price * m.Quantity)
                })
                .OrderByDescending(m => m.Sold)
                .Take(ListSize)
                .ToList();

            dto.LowStock = products
                .Where(m => m.Stock < LowStockLimit)
                .OrderBy(m => m.Stock)
                .Select(m => new LowStockDto
                {
                    ProductId = m.Id,
                    Name = m.Name,
                    Stock = m.Stock
                })
                .ToList();

            dto.RecentOrders = orders
                .Take(ListSize)
                .Select(OrderService.ToAdminDto)
                .ToList();

            return dto;
        }
    }
}
