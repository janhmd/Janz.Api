using Domain.Common;

namespace Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImagePath { get; set; } = string.Empty;
        public int Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
