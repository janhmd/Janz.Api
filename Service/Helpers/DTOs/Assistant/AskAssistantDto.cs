namespace Service.Helpers.DTOs.Assistant
{
    public class AskAssistantDto
    {
        public string Question { get; set; } = string.Empty;

        // Cavab dili, ziyarətçinin seçdiyi: en, az və ya ru
        public string? Language { get; set; }
    }
}
