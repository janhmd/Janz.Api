namespace Domain.Constants;

// Paketdəki qəhvə üçün üyütmə seçimi (səbətə və sifarişə yazılır, qiyməti dəyişmir)
public static class GrindOptions
{
    public const string WholeBeans = "Whole beans";

    public static readonly string[] All =
    {
        WholeBeans,
        "For French press (coarse)",
        "For drip coffee maker (coarse)",
        "For Chemex (coarse)",
        "For pour-over (medium)",
        "For AeroPress (medium)",
        "For moka pot (medium)",
        "For espresso, coarse (medium)",
        "For espresso, fine (fine)",
        "For Turkish coffee (fine)",
        "For a cup (medium)"
    };
}
