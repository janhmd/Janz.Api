namespace Service.Helpers.DTOs.Refunds
{
    // Admin paneldə sorğu: müştəri və sifarişin cəmi də göstərilir
    public class AdminRefundDto : RefundDto
    {
        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }
}
