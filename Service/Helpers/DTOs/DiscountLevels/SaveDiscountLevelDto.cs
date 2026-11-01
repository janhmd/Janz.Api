namespace Service.Helpers.DTOs.DiscountLevels
{
    // Cədvəldə bir sətir. Id boşdursa yeni səviyyədir
    public class SaveDiscountLevelDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public decimal MinSpent { get; set; }
        public int Percent { get; set; }
        public string? Perks { get; set; }
    }
}
