namespace Service.Helpers.DTOs.Balance
{
    public class BalanceTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
