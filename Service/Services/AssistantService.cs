using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Assistant;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Sayt köməkçisi: sual Google Gemini-yə (pulsuz API açarı) göndərilir (appsettings -> "Gemini"; ApiKey user-secrets-də saxlanılır).
    // Açar boşdursa və ya Gemini cavab vermirsə Answer boş qayıdır, sayt öz hazır cavablarını göstərir
    public class AssistantService : IAssistantService
    {
        private const string Instruction = "You are the assistant of Janz, an online coffee and tea shop in Baku, Azerbaijan. "
            + "Answer briefly (at most three sentences) in the language of the question. Help to choose coffee, brewing and the shop's service. "
            + "Facts: delivery in Baku takes 2-3 days and is free, regions get orders by Azerpost; pay by card online or cash or card to the courier; "
            + "promo codes are typed in the cart; order status is in the account under Orders; problems with an order go to WhatsApp +994 70 217 55 57. "
            + "Recommend only products from the list below and never invent products or prices. If the question is not about coffee, tea or the shop, politely say you only help with the shop.";

        private readonly IBaseRepository<Product> _productRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AssistantService(IBaseRepository<Product> productRepo,
                                IHttpClientFactory httpClientFactory,
                                IConfiguration configuration)
        {
            _productRepo = productRepo;
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

            var products = (await _productRepo.GetAllAsync())
                .Where(p => p.IsVisible)
                .Take(60)
                .Select(p => $"{p.Name} ({p.Price} AZN)");
            var instruction = Instruction + "\nProducts: " + string.Join("; ", products);

            var answer = await Gemini.AskAsync(_httpClientFactory, _configuration, instruction, question);

            return new AssistantAnswerDto { Answer = answer };
        }
    }
}
