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
    }
}
