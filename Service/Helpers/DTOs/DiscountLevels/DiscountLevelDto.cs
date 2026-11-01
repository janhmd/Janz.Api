namespace Service.Helpers.DTOs.DiscountLevels
{
    public class DiscountLevelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MinSpent { get; set; }
        public int Percent { get; set; }
        public string? Perks { get; set; }
        public int CustomerCount { get; set; }     // admin siyahısı üçün: bu səviyyədə neçə müştəri var
    }
}
