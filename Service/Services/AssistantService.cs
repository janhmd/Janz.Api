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
            + "Keep answers short, at most three sentences. "
            + "You talk only about Janz: its products, choosing coffee or tea and brewing what the shop sells, ordering, delivery, payment, promo codes, the account, balance, journal and community on this site. "
            + "For anything else (general knowledge, programming, news, politics, other shops, personal advice) reply with one short sentence, in the answer language, that you can only help with Janz. "
            + "Facts: delivery in Baku takes 2-3 days and is free, regions get orders by Azerpost; pay by card online or cash or card to the courier; "
            + "promo codes are typed in the cart; order status is in the account under Orders; problems with an order go to WhatsApp +994 70 217 55 57. "
            + "Recommend only products from the list below and never invent products, prices or facts.";

        // Ziyarətçi dili widget-də seçir; cavab yalnız o dildə olur (qarışıq dil olmasın deyə)
        private static string LanguageRule(string? language)
        {
            var name = (language ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "az" => "Azerbaijani",
                "ru" => "Russian",
                _ => "English"
            };

            return $"The visitor chose {name} as the chat language. Answer ONLY in {name}, even if the question is written in another language, and never mix languages. "
                + "The visitor may write the question in any language (Azerbaijani, Russian, English, Turkish, with or without special letters, with typos): decide whether it is about Janz by its meaning, never by its language. "
                + "A question about coffee, tea, a brewing method, delivery time, payment or an order is always about Janz, so answer it. "
                + "Keep product names as they are written in the list.";
        }

        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<ProductVariant> _variantRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AssistantService(IBaseRepository<Product> productRepo,
                                IBaseRepository<ProductVariant> variantRepo,
                                IHttpClientFactory httpClientFactory,
                                IConfiguration configuration)
        {
            _productRepo = productRepo;
            _variantRepo = variantRepo;
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

            // Variantı olan məhsulun real qiyməti variantlardadır (ən ucuzu), Product.Price köhnə qala bilər
            var variants = (await _variantRepo.GetAllAsync()).ToLookup(v => v.ProductId);
            var products = (await _productRepo.GetAllAsync())
                .Where(p => p.IsVisible)
                .Take(60)
                .Select(p => variants[p.Id].Any()
                    ? $"{p.Name} (from {variants[p.Id].Min(v => v.Price)} AZN)"
                    : $"{p.Name} ({p.Price} AZN)");
            var instruction = Instruction + "\n" + LanguageRule(model.Language) + "\nProducts: " + string.Join("; ", products);

            var answer = await Gemini.AskAsync(_httpClientFactory, _configuration, instruction, question);

            return new AssistantAnswerDto { Answer = answer };
        }
    }
}
