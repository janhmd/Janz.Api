namespace Service.Helpers.DTOs.Balance
{
    // Kartla artırmanın nəticəsi: ödəniş təsdiqləndisə məbləğ balansa yazılıb
    public class TopUpResultDto
    {
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
