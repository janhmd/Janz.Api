namespace Service.Helpers.DTOs.Refunds
{
    // Admin/Support: qəbul edəndə məbləği azalda bilər (boşdursa sifarişin cəmi), rədd edəndə səbəb yazılır
    public class ResolveRefundDto
    {
        public decimal? Amount { get; set; }
        public string? Note { get; set; }
    }
}
