using Domain.Common;

namespace Domain.Entities
{
    public class PostImage : BaseEntity
    {
        public string ImagePath { get; set; } = string.Empty;
        public int Order { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
    }
}
