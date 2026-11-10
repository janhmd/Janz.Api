using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        // Gözləyən sifarişin Stripe sessiyası bu qədər dəqiqə açıq qalır (Stripe-ın minimumu 30-dur), sonra təmizləmə işləyir
        private const int AwaitingSessionMinutes = 31;
        private const int StaleAfterMinutes = 35;

        private readonly IOrderRepository _orderRepo;
        private readonly IBalanceRepository _balanceRepo;
        private readonly IValidator<TopUpDto> _topUpValidator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly INotificationService _notificationService;

        public PaymentService(IOrderRepository orderRepo,
                              IBalanceRepository balanceRepo,
                              IValidator<TopUpDto> topUpValidator,
                              IHttpClientFactory httpClientFactory,
                              IConfiguration configuration,
                              ILogger<PaymentService> logger,
                              INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
            _orderRepo = orderRepo;
            _balanceRepo = balanceRepo;
            _topUpValidator = topUpValidator;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<ServiceResult<PaymentSessionDto>> CreateSessionAsync(string userId, int orderId)
        {
            var order = await _orderRepo.GetAnyByIdAsync(orderId);
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
            // Balansdan ödənən hissə çıxılır: kartla yalnız qalan məbləğ ödənir
            var amount = ((long)Math.Round((order.Total - order.BalanceUsed) * 100)).ToString(CultureInfo.InvariantCulture);
            var id = order.Id.ToString(CultureInfo.InvariantCulture);

            // Ödənişi gözləyən sifarişdə geri qayıtmaq sifarişi ləğv edir (səbət qalır), adi sifarişdə sifariş səhifəsinə qaytarır
            var cancelUrl = order.IsAwaitingPayment ? $"{baseUrl}/Orders/PaymentCancelled/{id}" : $"{baseUrl}/Orders/Detail/{id}";

            var form = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = $"{baseUrl}/Orders/Paid/{id}?session_id={{CHECKOUT_SESSION_ID}}",
                ["cancel_url"] = cancelUrl,
                ["client_reference_id"] = id,
                ["line_items[0][quantity]"] = "1",
                ["line_items[0][price_data][currency]"] = currency,
                ["line_items[0][price_data][unit_amount]"] = amount,
                ["line_items[0][price_data][product_data][name]"] = $"Janz order #{id}"
            };

            // Stripe sessiyası ən azı 30 dəqiqə açıq qala bilər: gözləyən sifariş bundan sonra silinir, sessiya isə artıq ödənilə bilmir
            if (order.IsAwaitingPayment)
            {
                form["expires_at"] = DateTimeOffset.UtcNow.AddMinutes(AwaitingSessionMinutes).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
            }

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

        public async Task RefreshAsync(string userId, int orderId)
        {
            var order = await _orderRepo.GetAnyByIdAsync(orderId);
            if (order is null || order.UserId != userId || order.IsPaid || string.IsNullOrWhiteSpace(order.PaymentSessionId))
            {
                return;
            }

            await ConfirmAsync(userId, orderId, new ConfirmPaymentDto { SessionId = order.PaymentSessionId });
        }

        public async Task<ServiceResult<PaymentResultDto>> ConfirmAsync(string userId, int orderId, ConfirmPaymentDto model)
        {
            var order = await _orderRepo.GetAnyByIdAsync(orderId);
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
                && root.GetProperty("amount_total").GetInt64() == (long)Math.Round((order.Total - order.BalanceUsed) * 100);

            if (paid)
            {
                if (order.IsAwaitingPayment)
                {
                    // Sifariş indi həqiqi olur: səbət boşalır, müştəriyə bildiriş gedir
                    if (await _orderRepo.FinalizePaidAsync(order.Id))
                    {
                        _logger.LogInformation("Card payment received: order #{OrderId} placed by user {UserId}", order.Id, order.UserId);
                        await NotifyPlacedAsync(order);
                    }
                }
                else
                {
                    order.IsPaid = true;
                    order.PaidAt = DateTime.UtcNow;
                    await _orderRepo.UpdateAsync(order);
                }
            }

            return ServiceResult<PaymentResultDto>.Ok(new PaymentResultDto { IsPaid = paid });
        }

        public async Task<ServiceResult<PaymentResultDto>> AbortAsync(string userId, int orderId)
        {
            var order = await _orderRepo.GetAnyByIdAsync(orderId);
            if (order is null || order.UserId != userId)
            {
                return ServiceResult<PaymentResultDto>.NotFound();
            }

            var paid = await SettleAsync(order);
            return ServiceResult<PaymentResultDto>.Ok(new PaymentResultDto { IsPaid = paid == true });
        }

        public async Task SettleAwaitingAsync(string userId)
        {
            foreach (var order in await _orderRepo.GetAwaitingAsync(userId, null))
            {
                // Sessiya hələ yaranmayıb və sifariş təzədirsə (eyni anda başlayan ödəniş) toxunulmur
                if (string.IsNullOrWhiteSpace(order.PaymentSessionId) && order.CreatedAt > DateTime.UtcNow.AddMinutes(-2))
                {
                    continue;
                }

                await SettleAsync(order);
            }
        }

        public async Task<int> SettleStaleAsync()
        {
            var settled = 0;
            foreach (var order in await _orderRepo.GetAwaitingAsync(null, DateTime.UtcNow.AddMinutes(-StaleAfterMinutes)))
            {
                if (await SettleAsync(order) is not null)
                {
                    settled++;
                }
            }

            return settled;
        }

        // true: ödənilib, sifariş həqiqi oldu. false: ödənilməyib, sifariş silindi. null: Stripe-dan dəqiq cavab alınmadı, sonra yenidən baxılır
        private async Task<bool?> SettleAsync(Order order)
        {
            if (!order.IsAwaitingPayment)
            {
                return order.IsPaid;
            }

            var state = string.IsNullOrWhiteSpace(order.PaymentSessionId) ? SessionState.Expired : await GetSessionStateAsync(order);

            if (state == SessionState.Paid)
            {
                if (await _orderRepo.FinalizePaidAsync(order.Id))
                {
                    _logger.LogInformation("Card payment received: order #{OrderId} placed by user {UserId}", order.Id, order.UserId);
                    await NotifyPlacedAsync(order);
                }
                return true;
            }

            if (state == SessionState.Open)
            {
                // Müştəri Stripe səhifəsini hələ açıq saxlaya bilər: sessiya bağlanır ki, sifariş silindikdən sonra pul çəkilməsin
                var secretKey = _configuration["Stripe:SecretKey"] ?? string.Empty;
                using var expired = await Send(HttpMethod.Post, $"{StripeUrl}/{Uri.EscapeDataString(order.PaymentSessionId!)}/expire", secretKey, new FormUrlEncodedContent(new Dictionary<string, string>()));
                if (expired is null)
                {
                    return null;
                }
                state = SessionState.Expired;
            }

            if (state == SessionState.Expired)
            {
                if (await _orderRepo.DiscardAsync(order.Id))
                {
                    _logger.LogInformation("Payment was not completed: order #{OrderId} of user {UserId} was removed, the cart is kept", order.Id, order.UserId);
                }
                return false;
            }

            return null;
        }

        private enum SessionState { Unknown, Paid, Open, Expired }

        private async Task<SessionState> GetSessionStateAsync(Order order)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return SessionState.Expired;
            }

            using var session = await Send(HttpMethod.Get, $"{StripeUrl}/{Uri.EscapeDataString(order.PaymentSessionId!)}", secretKey, null);
            if (session is null)
            {
                return SessionState.Unknown;
            }

            var root = session.RootElement;
            var paid = root.GetProperty("payment_status").GetString() == "paid"
                && root.GetProperty("client_reference_id").GetString() == order.Id.ToString(CultureInfo.InvariantCulture)
                && root.GetProperty("amount_total").GetInt64() == (long)Math.Round((order.Total - order.BalanceUsed) * 100);
            if (paid)
            {
                return SessionState.Paid;
            }

            return root.GetProperty("status").GetString() switch
            {
                "open" => SessionState.Open,
                "expired" => SessionState.Expired,
                _ => SessionState.Unknown
            };
        }

        private Task NotifyPlacedAsync(Order order)
        {
            return _notificationService.AddAsync(order.UserId, NotificationKind.Shop, $"Order #{order.Id} was placed",
                $"We received your order for ₼{order.Total:0.00}.", $"/Orders/Detail/{order.Id}");
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
                    // Səbəb API çıxışında görünsün (səhv açar, valyuta və s.)
                    _logger.LogWarning("Stripe returned {Status}: {Body}", (int)response.StatusCode, await response.Content.ReadAsStringAsync());
                    return null;
                }

                return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Stripe could not be reached");
                return null;
            }
        }
    }
}
