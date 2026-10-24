namespace Domain.Constants;

// Məhsul kartında görünən nişan. Boş olarsa nişan yoxdur
public static class ProductBadges
{
    public const string CoffeeOfTheWeek = "Coffee of the week"; // ana səhifədə "Coffees of the week" blokuna düşür
    public const string New = "New";
    public const string Sale = "Sale";                           // köhnə qiymət də göstərilir
    public const string Limited = "Limited";

    public static readonly string[] All = { CoffeeOfTheWeek, New, Sale, Limited };
}
