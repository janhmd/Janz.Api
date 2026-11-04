namespace Service.Helpers
{
    // Community-nin sabit siyahıları (yazı yazma formasındakı seçimlərlə eynidir)
    public static class CommunityRules
    {
        public const string ImageFolder = "community";
        public const int MaxImages = 10;

        public static readonly string[] Tags = { "Pour over", "Espresso", "Filter", "cezve", "French press", "AeroPress", "Drip", "Chemex", "Cold brew", "Light roast", "Dark roast" };
        public static readonly string[] BrewMethods = { "Espresso machine", "Drip coffee maker", "cezve", "French press", "AeroPress", "Pour over", "Chemex", "Moka pot", "Cold brew" };
        public static readonly string[] Tabs = { "best", "new" };
        public static readonly string[] Scopes = { "everywhere", "posts", "comments" };
    }
}
