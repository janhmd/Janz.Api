namespace Service.Helpers.DTOs.Assistant
{
    // Answer boş olanda Gemini qoşulmayıb və ya cavab verməyib: sayt öz hazır cavablarına qayıdır
    public class AssistantAnswerDto
    {
        public string Answer { get; set; } = string.Empty;
    }
}
