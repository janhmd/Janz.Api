namespace Service.Helpers.DTOs.Orders
{
    // Admin paneldə sifariş: müştərinin özü və hansı statuslara keçə biləcəyi də göstərilir
    public class AdminOrderDto : OrderDto
    {
        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public List<string> NextStatuses { get; set; } = new List<string>();
    }
}
