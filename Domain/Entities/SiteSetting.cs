using Domain.Common;

namespace Domain.Entities
{
    // Saytın əlaqə məlumatları: tək sətir (Id = 1), footer-də, Contacts bölməsində və köməkçidə göstərilir
    public class SiteSetting : BaseEntity
    {
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
    }
}
