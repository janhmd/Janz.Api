namespace Service.Helpers.DTOs.Balance
{
    // Kabinetdəki "Internal account": balans və hərəkətlər
    public class BalanceDto
    {
        public decimal Balance { get; set; }
        public List<BalanceTransactionDto> Transactions { get; set; } = new List<BalanceTransactionDto>();
    }
}
