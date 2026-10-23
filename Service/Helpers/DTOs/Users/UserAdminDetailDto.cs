using Service.Helpers.DTOs.Orders;

namespace Service.Helpers.DTOs.Users
{
    // Admin: bir istifadəçinin səhifəsi — məlumatları və sifarişləri
    public class UserAdminDetailDto : UserAdminDto
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
