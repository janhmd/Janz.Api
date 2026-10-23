using Domain.Enums;

namespace Service.Helpers
{
    // Sifarişin statusu yalnız irəli, bir addım gedir: Pending -> Processing -> Shipped -> Delivered.
    // Ləğv (Cancelled) yalnız göndərilməmiş sifarişdə olur. Delivered və Cancelled son statuslardır
    public static class OrderStatusFlow
    {
        public static IReadOnlyList<OrderStatus> NextOf(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
                OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
                OrderStatus.Shipped => new[] { OrderStatus.Delivered },
                _ => Array.Empty<OrderStatus>()
            };
        }

        public static bool CanMove(OrderStatus from, OrderStatus to)
        {
            return NextOf(from).Contains(to);
        }
    }
}
