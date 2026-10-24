namespace Domain.Constants;

// Məhsul səhifəsindəki "Brewing" bloku: məhsulun hansı üsullarla hazırlana bildiyi
public static class BrewMethods
{
    public const string EspressoMachine = "Espresso machine";
    public const string TurkishCoffee = "Turkish coffee";
    public const string MokaPot = "Moka pot";
    public const string PourOver = "Pour over";
    public const string FrenchPress = "French press";
    public const string AeroPress = "AeroPress";
    public const string DripCoffeeMaker = "Drip coffee maker";
    public const string Cup = "Cup";
    public const string CapsuleMachine = "Capsule machine";

    public static readonly string[] All =
    {
        EspressoMachine, TurkishCoffee, MokaPot, PourOver, FrenchPress, AeroPress, DripCoffeeMaker, Cup, CapsuleMachine
    };
}
