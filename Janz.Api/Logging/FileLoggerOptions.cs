namespace Janz.Api.Logging
{
    // appsettings.json -> "FileLogging"
    public class FileLoggerOptions
    {
        // Qovluq (API qovluğuna görə). Hər gün ayrıca fayl: janz-20261110.log
        public string Path { get; set; } = "logs";

        // Bu qədər gündən köhnə fayllar silinir
        public int RetainDays { get; set; } = 14;

        // Warning və yuxarı hadisələr əlavə olaraq errors-20261110.log faylına da yazılır: xətalara tez baxmaq üçün
        public bool SeparateErrorFile { get; set; } = true;
    }
}
