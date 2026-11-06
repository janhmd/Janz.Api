using Microsoft.Extensions.Configuration;
using Service.Helpers;
using Service.Helpers.DTOs.Assistant;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Admin panelin köməkçisi: işçinin sualına Gemini cavab verir, cari rəqəmlər (dashboard) sualın yanında göndərilir.
    // Açar yoxdursa Answer boşdur, panel öz hazır cavablarını göstərir
    public class AdminAssistantService : IAdminAssistantService
    {
        private const string Instruction = "You help the staff of Janz, an online coffee and tea shop, to work in its admin panel. "
            + "Answer briefly (at most four sentences) in the language of the question. "
            + "The panel sections: Products (add or edit a product, price, stock, images, visibility), Categories, Tags, Orders (open an order and change its status), "
            + "Coupons (promo codes), Gift certificates, Discount levels (automatic discount for regular customers), Journal (articles), Community (customer posts, can be hidden), "
            + "Pages (Terms and other site pages), Users, Roles (super admin only), Reviews, Settings (phone, address, footer texts). "
            + "Use the current numbers below when the question is about the shop's state and never invent numbers. "
            + "If the question is not about running the shop or the panel, politely say you only help with the admin work.";

        private readonly IDashboardService _dashboardService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AdminAssistantService(IDashboardService dashboardService,
                                     IHttpClientFactory httpClientFactory,
                                     IConfiguration configuration)
        {
            _dashboardService = dashboardService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AssistantAnswerDto> AskAsync(AskAssistantDto model)
        {
            var question = model.Question?.Trim() ?? string.Empty;
            if (question.Length == 0 || question.Length > 300)
            {
                throw new BadRequestException("Write a question up to 300 characters.");
            }

            var dashboard = await _dashboardService.GetAsync();
            var lowStock = dashboard.LowStock.Select(p => $"{p.Name} ({p.Stock})");
            var numbers = $"\nCurrent numbers: pending orders {dashboard.PendingOrders}; orders today {dashboard.OrdersToday}; total orders {dashboard.OrderCount}; "
                + $"revenue this month {dashboard.RevenueThisMonth} AZN; new customers this month {dashboard.NewCustomersThisMonth}; products {dashboard.ProductCount}; "
                + "low stock: " + (dashboard.LowStock.Count == 0 ? "none" : string.Join(", ", lowStock));

            var answer = await Gemini.AskAsync(_httpClientFactory, _configuration, Instruction + numbers, question);

            return new AssistantAnswerDto { Answer = answer };
        }
    }
}
