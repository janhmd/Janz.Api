using Service.Helpers.DTOs.Orders;

namespace Service.Helpers.DTOs.Dashboard
{
    // Admin panelin ana səhifəsi: hamısı real sifariş və məhsullardan hesablanır
    public class DashboardDto
    {
        public decimal TotalRevenue { get; set; }            // ləğv olunmamış bütün sifarişlər
        public decimal RevenueThisMonth { get; set; }
        public int OrderCount { get; set; }
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }               // hələ işə götürülməyənlər: ilk baxılmalı olanlar
        public decimal AverageOrder { get; set; }
        public int CustomerCount { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ProductCount { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        public List<DailySalesDto> SalesLast7Days { get; set; } = new List<DailySalesDto>();
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
        public List<LowStockDto> LowStock { get; set; } = new List<LowStockDto>();
        public List<AdminOrderDto> RecentOrders { get; set; } = new List<AdminOrderDto>();
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int Orders { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
