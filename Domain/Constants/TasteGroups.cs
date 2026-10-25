namespace Domain.Constants
{
    // Kataloqdakı "Taste" filtrinin qrupları (məhsulun notlarına görə admin seçir)
    public static class TasteGroups
    {
        public const string ChocolateNuts = "Chocolate / Nuts";
        public const string FlowersCitrus = "Flowers / Citrus";
        public const string FruitsBerries = "Fruits / Berries";
        public const string AlcoholFunky = "Alcohol / Funky";

        public static readonly string[] All = { ChocolateNuts, FlowersCitrus, FruitsBerries, AlcoholFunky };
    }
}
