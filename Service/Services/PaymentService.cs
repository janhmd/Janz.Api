using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Balance;
using Service.Helpers.DTOs.Payments;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Kartla ödəniş Stripe Checkout ilə (appsettings -> "Stripe"; SecretKey user-secrets-də saxlanılır).
    // Stripe-ın REST API-si birbaşa çağırılır: sessiya yaradılır, müştəri qayıdanda sessiyanın statusu yoxlanılır
    public class PaymentService : IPaymentService
    {
        private const string StripeUrl = "https://api.stripe.com/v1/checkout/sessions";

        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IBalanceRepository _balanceRepo;
        private readonly IValidator<TopUpDto> _topUpValidator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentService(IBaseRepository<Order> orderRepo,
                              IBalanceRepository balanceRepo,
                              IValidator<TopUpDto> topUpValidator,
                              IHttpClientFactory httpClientFactory,
                              IConfiguration configuration)
        {
            _orderRepo = orderRepo;
            _balanceRepo = balanceRepo;
            _topUpValidator = topUpValidator;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<ServiceResult<PaymentSessionDto>> CreateSessionAsync(string userId, int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order is null || order.UserId != userId)
            {
                return ServiceResult<PaymentSessionDto>.NotFound();
            }

            if (order.IsPaid)
            {
                return ServiceResult<PaymentSessionDto>.Fail("This order is already paid.");
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            {
                return ServiceResult<PaymentSessionDto>.Fail("This order can't be paid now.");
            }

            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return ServiceResult<PaymentSessionDto>.Fail("Online payment is not set up yet. You can pay on delivery.");
            }

            // Stripe məbləği qəpiklə qəbul edir; geri qayıdış ünvanları MVC saytındadır
            var baseUrl = (_configuration["Frontend:MvcBaseUrl"] ?? "https://localhost:7047").TrimEnd('/');
            var currency = _configuration["Stripe:Currency"] ?? "azn";
            var amount = ((long)Math.Round(order.Total * 100)).ToString(CultureInfo.InvariantCulture);
            var id = order.Id.ToString(CultureInfo.InvariantCulture);

            var form = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = $"{baseUrl}/Orders/Paid/{id}?session_id={{CHECKOUT_SESSION_ID}}",
                ["cancel_url"] = $"{baseUrl}/Orders/Detail/{id}",
                ["client_reference_id"] = id,
                ["line_items[0][quantity]"] = "1",
                ["line_items[0][price_data][currency]"] = currency,
                ["line_items[0][price_data][unit_amount]"] = amount,
                ["line_items[0][price_data][product_data][name]"] = $"Janz order #{id}"
            };

            using var response = await Send(HttpMethod.Post, StripeUrl, secretKey, new FormUrlEncodedContent(form));
            if (response is null)
            {
                return ServiceResult<PaymentSessionDto>.Fail("The payment could not be started. Please try again.");
            }

            var sessionId = response.RootElement.GetProperty("id").GetString();
            var url = response.RootElement.GetProperty("url").GetString();

            order.PaymentSessionId = sessionId;
            await _orderRepo.UpdateAsync(order);

            return ServiceResult<PaymentSessionDto>.Ok(new PaymentSessionDto { Url = url ?? string.Empty });
        }

        public async Task<ServiceResult<PaymentResultDto>> ConfirmAsync(string userId, int orderId, ConfirmPaymentDto model)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order is null || order.UserId != userId)
            {
                return ServiceResult<PaymentResultDto>.NotFound();
            }

            if (order.IsPaid)
            {
                return ServiceResult<PaymentResultDto>.Ok(new PaymentResultDto { IsPaid = true });
            }

            // Yalnız bu sifariş üçün yaradılmış sessiya qəbul olunur (ünvandakı session_id-yə güvənmirik)
            if (string.IsNullOrWhiteSpace(model.SessionId) || model.SessionId != order.PaymentSessionId)
            {
                return ServiceResult<PaymentResultDto>.Fail("This payment does not belong to the order.");
            }

            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return ServiceResult<PaymentResultDto>.Fail("Online payment is not set up yet.");
            }

            using var session = await Send(HttpMethod.Get, $"{StripeUrl}/{Uri.EscapeDataString(model.SessionId)}", secretKey, null);
            if (session is null)
            {
                return ServiceResult<PaymentResultDto>.Fail("The payment could not be checked. Please try again.");
            }

            var root = session.RootElement;
            var paid = root.GetProperty("payment_status").GetString() == "paid"
                && root.GetProperty("client_reference_id").GetString() == order.Id.ToString(CultureInfo.InvariantCulture)
                && root.GetProperty("amount_total").GetInt64() == (long)Math.Round(order.Total * 100);

            if (paid)
            {
                order.IsPaid = true;
                order.PaidAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(order);
            }

            return ServiceResult<PaymentResultDto>.Ok(new PaymentResultDto { IsPaid = paid });
        }

        public async Task<ServiceResult<PaymentSessionDto>> CreateTopUpSessionAsync(string userId, TopUpDto model)
        {
            var validation = await _topUpValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<PaymentSessionDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return ServiceResult<PaymentSessionDto>.Fail("Online payment is not set up yet.");
            }

            var baseUrl = (_configuration["Frontend:MvcBaseUrl"] ?? "https://localhost:7047").TrimEnd('/');
            var currency = _configuration["Stripe:Currency"] ?? "azn";
            var amount = ((long)Math.Round(model.Amount * 100)).ToString(CultureInfo.InvariantCulture);

            var form = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = $"{baseUrl}/Profile/TopUpPaid?session_id={{CHECKOUT_SESSION_ID}}",
                ["cancel_url"] = $"{baseUrl}/Profile#ledger",
                ["client_reference_id"] = userId,
                ["line_items[0][quantity]"] = "1",
                ["line_items[0][price_data][currency]"] = currency,
                ["line_items[0][price_data][unit_amount]"] = amount,
                ["line_items[0][price_data][product_data][name]"] = "Janz balance top-up"
            };

            using var response = await Send(HttpMethod.Post, StripeUrl, secretKey, new FormUrlEncodedContent(form));
            if (response is null)
            {
                return ServiceResult<PaymentSessionDto>.Fail("The payment could not be started. Please try again.");
            }

            var sessionId = response.RootElement.GetProperty("id").GetString() ?? string.Empty;
            var url = response.RootElement.GetProperty("url").GetString();

            // Ödəniş təsdiqlənənə qədər balansa sayılmır
            await _balanceRepo.AddAsync(new BalanceTransaction
            {
                UserId = userId,
                Amount = Math.Round(model.Amount, 2),
                Type = BalanceTransactionType.TopUp,
                Note = "Top-up by card",
                PaymentSessionId = sessionId,
                IsConfirmed = false
            });

            return ServiceResult<PaymentSessionDto>.Ok(new PaymentSessionDto { Url = url ?? string.Empty });
        }

        public async Task<ServiceResult<TopUpResultDto>> ConfirmTopUpAsync(string userId, ConfirmTopUpDto model)
        {
            if (string.IsNullOrWhiteSpace(model.SessionId))
            {
                return ServiceResult<TopUpResultDto>.NotFound();
            }

            // Yalnız bu istifadəçinin yaratdığı sessiya qəbul olunur
            var entry = await _balanceRepo.GetBySessionAsync(userId, model.SessionId);
            if (entry is null)
            {
                return ServiceResult<TopUpResultDto>.NotFound();
            }

            if (!entry.IsConfirmed)
            {
                var secretKey = _configuration["Stripe:SecretKey"];
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    return ServiceResult<TopUpResultDto>.Fail("Online payment is not set up yet.");
                }

                using var session = await Send(HttpMethod.Get, $"{StripeUrl}/{Uri.EscapeDataString(model.SessionId)}", secretKey, null);
                if (session is null)
                {
                    return ServiceResult<TopUpResultDto>.Fail("The payment could not be checked. Please try again.");
                }

                var root = session.RootElement;
                var paid = root.GetProperty("payment_status").GetString() == "paid"
                    && root.GetProperty("client_reference_id").GetString() == userId
                    && root.GetProperty("amount_total").GetInt64() == (long)Math.Round(entry.Amount * 100);

                if (paid)
                {
                    entry.IsConfirmed = true;
                    await _balanceRepo.UpdateAsync(entry);
                }
            }

            return ServiceResult<TopUpResultDto>.Ok(new TopUpResultDto
            {
                IsPaid = entry.IsConfirmed,
                Amount = entry.Amount,
                Balance = await _balanceRepo.GetBalanceAsync(userId)
            });
        }

        // Stripe cavab verməsə və ya xəta qaytarsa null
        private async Task<JsonDocument?> Send(HttpMethod method, string url, string secretKey, HttpContent? content)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url) { Content = content };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                using var response = await _httpClientFactory.CreateClient().SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
