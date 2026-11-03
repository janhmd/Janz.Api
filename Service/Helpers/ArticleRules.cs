namespace Service.Helpers
{
    // Jurnalın mövzu və teq siyahısı (saytın sol menyusu ilə eynidir)
    public static class ArticleRules
    {
        public const string ImageFolder = "articles";
        public const int WordsPerMinute = 200;

        public static readonly string[] Topics = { "Brewing guides", "Brewing", "Industry", "Growing", "Roasting", "Marketing", "Tea" };
        public static readonly string[] Tags = { "Growing", "Varieties", "Roasting", "Brewing", "Industry", "Marketing", "Espresso" };
    }
}
