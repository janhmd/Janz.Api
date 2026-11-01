namespace Service.Helpers.DTOs.DiscountLevels
{
    // Müştərinin səviyyəsi: səbətdə və hesabda göstərilir
    public class MyDiscountLevelDto
    {
        public string? Name { get; set; }
        public int Percent { get; set; }
        public decimal Spent { get; set; }
        public string? NextName { get; set; }
        public decimal? NextMinSpent { get; set; }
    }
}
