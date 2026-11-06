using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Service.Helpers
{
    // Google Gemini-yə tək sual göndərir (açar appsettings -> "Gemini"; ApiKey user-secrets-də saxlanılır).
    // Açar boşdursa və ya Gemini cavab vermirsə boş mətn qayıdır, çağıran öz hazır cavabına keçir
    public static class Gemini
    {
        // "-latest" həmişə Google-un cari Flash modelinə yönləndirir (köhnə model adları bağlananda köməkçi dayanmasın)
        private const string DefaultModel = "gemini-flash-latest";

        public static async Task<string> AskAsync(IHttpClientFactory httpClientFactory, IConfiguration configuration, string instruction, string question)
        {
            var key = configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var geminiModel = configuration["Gemini:Model"] ?? DefaultModel;
            var request = new
            {
                system_instruction = new { parts = new[] { new { text = instruction } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = question } } } },
                generationConfig = new { maxOutputTokens = 300, temperature = 0.4 }
            };

            try
            {
                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                using var message = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent");
                message.Headers.Add("x-goog-api-key", key);
                message.Content = JsonContent.Create(request);

                using var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                {
                    var failure = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Gemini request failed: {(int)response.StatusCode} {geminiModel} {failure[..Math.Min(failure.Length, 300)]}");
                    return string.Empty;
                }

                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (!json.RootElement.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
                {
                    return string.Empty;
                }

                var parts = candidates[0].GetProperty("content").GetProperty("parts");
                var text = parts.GetArrayLength() == 0 ? null : parts[0].GetProperty("text").GetString();

                return text?.Trim() ?? string.Empty;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
            {
                Console.Error.WriteLine($"Gemini request error: {ex.GetType().Name} {ex.Message}");
                return string.Empty;
            }
        }
    }
}
