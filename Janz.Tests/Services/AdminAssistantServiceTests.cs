using System.Net;
using Janz.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Service.Helpers.DTOs.Assistant;
using Service.Helpers.DTOs.Dashboard;
using Service.Helpers.Exceptions;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class AdminAssistantServiceTests
    {
        private const string GeminiAnswer = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\" Open Orders. \"}]}}]}";

        private static AdminAssistantService Create(FakeHttpClientFactory http, string? apiKey)
        {
            var dashboard = new FakeDashboardService();
            dashboard.Dashboard.PendingOrders = 7;
            dashboard.Dashboard.LowStock.Add(new LowStockDto { Name = "Aroma kit", Stock = 2 });

            var settings = new Dictionary<string, string?> { ["Gemini:ApiKey"] = apiKey };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            return new AdminAssistantService(dashboard, http, configuration);
        }

        [Fact]
        public async Task Ask_ReturnsEmptyAnswer_WhenNoApiKey()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);

            var answer = await Create(http, null).AskAsync(new AskAssistantDto { Question = "What is pending?" });

            Assert.Equal(string.Empty, answer.Answer);
            Assert.Equal(0, http.Calls);
        }

        [Fact]
        public async Task Ask_ThrowsBadRequest_ForEmptyQuestion()
        {
            var service = Create(new FakeHttpClientFactory(), "key");

            await Assert.ThrowsAsync<BadRequestException>(() => service.AskAsync(new AskAssistantDto { Question = " " }));
        }

        [Fact]
        public async Task Ask_SendsDashboardNumbersToGemini()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);

            var answer = await Create(http, "key").AskAsync(new AskAssistantDto { Question = "What is pending?" });

            Assert.Equal("Open Orders.", answer.Answer);
            Assert.Contains("pending orders 7", http.LastRequestBody!);
            Assert.Contains("Aroma kit (2)", http.LastRequestBody!);
        }

        [Fact]
        public async Task Ask_ReturnsEmptyAnswer_WhenGeminiFails()
        {
            var answer = await Create(new FakeHttpClientFactory(HttpStatusCode.TooManyRequests), "key")
                .AskAsync(new AskAssistantDto { Question = "What is pending?" });

            Assert.Equal(string.Empty, answer.Answer);
        }
    }
}
