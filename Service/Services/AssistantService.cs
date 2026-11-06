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
        private const string Instruction = "You are the assistant on the website of Janz, an online coffee and tea shop in Baku, Azerbaijan. "
            + "Always answer in the same language as the user's last message (Azerbaijani, Russian, English, Turkish or any other) and never switch to another language. "
            + "Keep answers short, at most three sentences. "
            + "You talk only about Janz: its products, choosing coffee or tea and brewing what the shop sells, ordering, delivery, payment, promo codes, the account, balance, journal and community on this site. "
            + "For anything else (general knowledge, programming, news, politics, other shops, personal advice) reply with one short sentence, in the user's language, that you can only help with Janz. "
            + "Facts: delivery in Baku takes 2-3 days and is free, regions get orders by Azerpost; pay by card online or cash or card to the courier; "
            + "promo codes are typed in the cart; order status is in the account under Orders; problems with an order go to WhatsApp +994 70 217 55 57. "
            + "Recommend only products from the list below and never invent products, prices or facts.";

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
