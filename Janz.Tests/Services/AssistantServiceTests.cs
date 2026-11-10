using System.Net;
using Domain.Entities;
using Janz.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Service.Helpers.DTOs.Assistant;
using Service.Helpers.Exceptions;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class AssistantServiceTests
    {
        private const string GeminiAnswer = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\" Try the Ethiopia. \"}]}}]}";

        private static AssistantService Create(FakeHttpClientFactory http, string? apiKey)
        {
            var products = new FakeRepository<Product>();
            products.Rows.Add(new Product { Id = 1, Name = "Ethiopia", Price = 29.9m, IsVisible = true });
            products.Rows.Add(new Product { Id = 2, Name = "Hidden roast", Price = 10m, IsVisible = false });

            var variants = new FakeRepository<ProductVariant>();
            variants.Rows.Add(new ProductVariant { Id = 1, ProductId = 1, Price = 24.5m });
            variants.Rows.Add(new ProductVariant { Id = 2, ProductId = 1, Price = 79m });

            var settings = new Dictionary<string, string?> { ["Gemini:ApiKey"] = apiKey };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            return new AssistantService(products, variants, http, configuration);
        }

        [Fact]
        public async Task Ask_ReturnsEmptyAnswer_WhenNoApiKey()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);
            var service = Create(http, null);

            var answer = await service.AskAsync(new AskAssistantDto { Question = "Which coffee?" });

            Assert.Equal(string.Empty, answer.Answer);
            Assert.Equal(0, http.Calls);
        }

        [Fact]
        public async Task Ask_ThrowsBadRequest_ForEmptyQuestion()
        {
            var service = Create(new FakeHttpClientFactory(), "key");

            await Assert.ThrowsAsync<BadRequestException>(() => service.AskAsync(new AskAssistantDto { Question = "  " }));
        }

        [Fact]
        public async Task Ask_ThrowsBadRequest_ForTooLongQuestion()
        {
            var service = Create(new FakeHttpClientFactory(), "key");

            await Assert.ThrowsAsync<BadRequestException>(() => service.AskAsync(new AskAssistantDto { Question = new string('a', 301) }));
        }

        [Fact]
        public async Task Ask_ReturnsGeminiText_AndSendsKeyInHeader()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);
            var service = Create(http, "secret-key");

            var answer = await service.AskAsync(new AskAssistantDto { Question = "Which coffee?" });

            Assert.Equal("Try the Ethiopia.", answer.Answer);
            Assert.Equal("secret-key", http.LastApiKey);
        }

        [Fact]
        public async Task Ask_SendsOnlyVisibleProductsToGemini()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);
            var service = Create(http, "key");

            await service.AskAsync(new AskAssistantDto { Question = "Which coffee?" });

            Assert.Contains("Ethiopia", http.LastRequestBody!);
            Assert.DoesNotContain("Hidden roast", http.LastRequestBody!);
        }

        [Fact]
        public async Task Ask_ReturnsEmptyAnswer_WhenGeminiFails()
        {
            var service = Create(new FakeHttpClientFactory(HttpStatusCode.TooManyRequests), "key");

            var answer = await service.AskAsync(new AskAssistantDto { Question = "Which coffee?" });

            Assert.Equal(string.Empty, answer.Answer);
        }

        [Fact]
        public async Task Ask_ReturnsEmptyAnswer_WhenGeminiReplyHasNoText()
        {
            var service = Create(new FakeHttpClientFactory(HttpStatusCode.OK, "{\"candidates\":[]}"), "key");

            var answer = await service.AskAsync(new AskAssistantDto { Question = "Which coffee?" });

            Assert.Equal(string.Empty, answer.Answer);
        }

        [Theory]
        [InlineData("az", "Answer ONLY in Azerbaijani")]
        [InlineData("RU", "Answer ONLY in Russian")]
        [InlineData("en", "Answer ONLY in English")]
        [InlineData("fr", "Answer ONLY in English")]
        [InlineData(null, "Answer ONLY in English")]
        public async Task Ask_AnswersInTheChosenLanguage(string? language, string expected)
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);

            await Create(http, "key").AskAsync(new AskAssistantDto { Question = "Which coffee?", Language = language });

            Assert.Contains(expected, http.LastRequestBody!);
        }

        [Fact]
        public async Task Ask_TellsGeminiToJudgeTopicByMeaningNotLanguage()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);

            await Create(http, "key").AskAsync(new AskAssistantDto { Question = "Qehve" });

            Assert.Contains("never by its language", http.LastRequestBody!);
        }

        [Fact]
        public async Task Ask_UsesTheCheapestVariantPrice_NotTheStaleProductPrice()
        {
            var http = new FakeHttpClientFactory(HttpStatusCode.OK, GeminiAnswer);

            await Create(http, "key").AskAsync(new AskAssistantDto { Question = "How much is Ethiopia?" });

            Assert.Contains("Ethiopia (from 24.5 AZN)", http.LastRequestBody!);
            Assert.DoesNotContain("29.9", http.LastRequestBody!);
        }
    }
}
