namespace Service.Helpers.DTOs.Orders
{
    // Admin: sifarişin yeni statusu (Processing, Shipped, Delivered və ya Cancelled)
    public class UpdateOrderStatusDto
    {
        public string? Status { get; set; }
    }
}
