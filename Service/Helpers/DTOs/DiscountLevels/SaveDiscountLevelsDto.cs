namespace Service.Helpers.DTOs.DiscountLevels
{
    // Admin bütün cədvəli birdən saxlayır: siyahıda olmayan səviyyələr silinir
    public class SaveDiscountLevelsDto
    {
        public List<SaveDiscountLevelDto> Levels { get; set; } = new List<SaveDiscountLevelDto>();
    }
}
