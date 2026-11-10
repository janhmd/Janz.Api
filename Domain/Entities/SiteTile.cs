using Domain.Common;

namespace Domain.Entities
{
    // Saytdakı şəkilli kart (Home-da və kataloq səhifələrinin yuxarısında). Admin paneldən dəyişir.
    // Şəkli yenisi ilə əvəz edəndə köhnə hover videosu silinir: şəkil və video bir-birinə uyğun olmalıdır
    public class SiteTile : BaseEntity
    {
        // Hansı səhifədə göstərilir (TilePlacements)
        public string Placement { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        // Basanda açılan ünvan: "/Products/Tea" və ya səhifənin özündə filtr ("?tag=oolong", "?use=espresso")
        public string Link { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
        public string? VideoPath { get; set; }

        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
