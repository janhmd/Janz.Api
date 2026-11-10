namespace Service.Helpers.DTOs.Orders
{
    public class CreateOrderDto
    {
        // İstifadəçinin öz ünvanlarından biri (Addresses). Sifarişə onun surəti yazılır
        public int AddressId { get; set; }

        // Səbətdə yazılan promo kod (istəyə bağlı)
        public string? CouponCode { get; set; }

        // Daxili hesabdakı puldan istifadə et (çatarsa sifariş tam ödənir, çatmazsa qalanı kartla və ya çatdırılmada)
        public bool UseBalance { get; set; }

        // "Card": pul sifariş yaranmazdan əvvəl Stripe-da alınır (alınmasa sifariş yaranmır, səbət qalır).
        // "Cash": çatdırılmada kuryerə ödənir, yalnız Bakıda. Boşdursa "Card"
        public string? PaymentMethod { get; set; }
    }
}
