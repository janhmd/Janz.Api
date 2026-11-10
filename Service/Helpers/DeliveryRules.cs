using Microsoft.Extensions.Configuration;

namespace Service.Helpers
{
    // Çatdırılma qaydaları (appsettings -> "Delivery"): haqq, pulsuz çatdırılma həddi və çatdırılmada ödəniş yalnız Bakıda.
    // Regionlara Azərpoçt göndərir: pul öncədən kartla alınır
    public class DeliveryRules
    {
        public decimal Fee { get; init; }
        public decimal FreeFrom { get; init; }

        // Kuryerə nağd/kartla ödəmək olan şəhərlər (ad, hərflər böyük-kiçik fərq etmir)
        private static readonly string[] CashCities = { "baku", "bakı", "баку" };

        public static DeliveryRules From(IConfiguration configuration)
        {
            return new DeliveryRules
            {
                Fee = configuration.GetValue("Delivery:Fee", 3m),
                FreeFrom = configuration.GetValue("Delivery:FreeFrom", 50m)
            };
        }

        public static bool IsCashAllowed(string? city)
        {
            return !string.IsNullOrWhiteSpace(city)
                && CashCities.Contains(city.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        // Endirimdən sonrakı məbləğə görə çatdırılma haqqı (müəyyən həddən yuxarı pulsuz)
        public decimal FeeFor(decimal amountAfterDiscount)
        {
            return FreeFrom > 0 && amountAfterDiscount >= FreeFrom ? 0 : Fee;
        }
    }
}
