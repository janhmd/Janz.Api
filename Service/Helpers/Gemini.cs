using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Service.Helpers
{
    // Google Gemini-yə tək sual göndərir (açar appsettings -> "Gemini"; ApiKey user-secrets-də saxlanılır).
    // Açar boşdursa və ya Gemini cavab vermirsə boş mətn qayıdır, çağıran öz hazır cavabına keçir
    public static class Gemini
    {
        // "-latest" həmişə Google-un cari modelinə yönləndirir (köhnə model adları bağlananda köməkçi dayanmasın).
        // Əsas model sürətli "lite"-dır (qısa cavablar üçün "düşünmə" gecikməsi yoxdur); o cavab verməsə daha güclü model yoxlanılır
        private const string DefaultModel = "gemini-flash-lite-latest";

        // Əsas model "high demand" (503) və ya limit (429) verəndə növbəti model yoxlanılır
        private static readonly string[] FallbackModels = { "gemini-flash-latest", "gemini-2.5-flash-lite" };

        public static async Task<string> AskAsync(IHttpClientFactory httpClientFactory, IConfiguration configuration, string instruction, string question)
        {
            return (await AskDetailedAsync(httpClientFactory, configuration, instruction, question)).Text;
        }

        // Cavabla birlikdə uğursuzluğun səbəbini də qaytarır (Error boşdursa hər şey qaydasındadır)
        public static async Task<(string Text, string Error)> AskDetailedAsync(IHttpClientFactory httpClientFactory, IConfiguration configuration, string instruction, string question)
        {
            var key = configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.Error.WriteLine("Gemini is off: Gemini:ApiKey is empty (set it with user-secrets). The site uses its ready answers.");
                return (string.Empty, "Gemini:ApiKey is empty");
            }

            var models = new List<string> { configuration["Gemini:Model"] ?? DefaultModel };
            models.AddRange(FallbackModels.Where(m => !models.Contains(m)));

            var lastError = string.Empty;
            foreach (var model in models)
            {
                var (text, error, tryNext) = await TryModelAsync(httpClientFactory, key, model, instruction, question);
                if (error.Length == 0)
                {
                    return (text, string.Empty);
                }

                lastError = error;
                if (!tryNext)
                {
                    break;
                }
            }

            return (string.Empty, lastError);
        }

        // TryNext: səhv müvəqqətidir (yüklənmə, limit, vaxt aşımı), başqa model yoxlamağa dəyər
        private static async Task<(string Text, string Error, bool TryNext)> TryModelAsync(IHttpClientFactory httpClientFactory, string key, string model, string instruction, string question)
        {
            var request = new
            {
                system_instruction = new { parts = new[] { new { text = instruction } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = question } } } },
                generationConfig = new { maxOutputTokens = 1024, temperature = 0.4 }
            };

            try
            {
                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                using var message = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent");
                message.Headers.Add("x-goog-api-key", key);
                message.Content = JsonContent.Create(request);

                using var response = await client.SendAsync(message);
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var failed = $"Gemini request failed: {(int)response.StatusCode} {model} {body[..Math.Min(body.Length, 300)]}";
                    Console.Error.WriteLine(failed);
                    var status = (int)response.StatusCode;
                    return (string.Empty, failed, status is 429 or 404 or >= 500);
                }

                using var json = JsonDocument.Parse(body);
                if (!json.RootElement.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
                {
                    var noCandidates = $"Gemini returned no candidates ({model}): {body[..Math.Min(body.Length, 300)]}";
                    Console.Error.WriteLine(noCandidates);
                    return (string.Empty, noCandidates, true);
                }

                // Yeni modellər cavabdan əvvəl "düşünür": mətn bir neçə hissədə ola bilər, "thought" hissələri atılır
                var text = new StringBuilder();
                if (candidates[0].TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        var isThought = part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True;
                        if (!isThought && part.TryGetProperty("text", out var piece) && piece.ValueKind == JsonValueKind.String)
                        {
                            text.Append(piece.GetString());
                        }
                    }
                }

                if (text.Length == 0)
                {
                    var noText = $"Gemini returned no text ({model}): {body[..Math.Min(body.Length, 300)]}";
                    Console.Error.WriteLine(noText);
                    return (string.Empty, noText, true);
                }

                return (text.ToString().Trim(), string.Empty, false);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
            {
                var error = $"Gemini request error ({model}): {ex.GetType().Name} {ex.Message}";
                Console.Error.WriteLine(error);
                return (string.Empty, error, true);
            }
        }
    }
}
