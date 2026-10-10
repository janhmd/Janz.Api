using Domain.Common;

namespace Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
