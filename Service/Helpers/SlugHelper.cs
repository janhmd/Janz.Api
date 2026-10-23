using System.Text;

namespace Service.Helpers
{
    // Addan URL üçün slug düzəldir: "Kofe dəstləri" -> "kofe-destleri"
    public static class SlugHelper
    {
        public const string Pattern = "^[a-z0-9]+(-[a-z0-9]+)*$";

        public static string Make(string text)
        {
            var map = new Dictionary<char, string>
            {
                ['ə'] = "e", ['ı'] = "i", ['ö'] = "o", ['ü'] = "u", ['ş'] = "s", ['ç'] = "c", ['ğ'] = "g"
            };

            var builder = new StringBuilder();
            foreach (var symbol in text.Trim().ToLowerInvariant())
            {
                if (map.TryGetValue(symbol, out var latin))
                {
                    builder.Append(latin);
                }
                else if (symbol is >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    builder.Append(symbol);
                }
                else if (builder.Length > 0 && builder[^1] != '-')
                {
                    // Boşluq və digər işarələr bir defisə çevrilir
                    builder.Append('-');
                }
            }

            return builder.ToString().Trim('-');
        }
    }
}
