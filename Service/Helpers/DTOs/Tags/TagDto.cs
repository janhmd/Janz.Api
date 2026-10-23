namespace Service.Helpers.DTOs.Tags
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }      // admin siyahısı üçün: neçə məhsul var
    }
}
