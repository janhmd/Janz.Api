using Domain.Entities;

namespace Repository.Results
{
    public enum OrderOutcome
    {
        Success,
        CartEmpty,
        InsufficientStock      // eyni anda başqa sifariş stoku azaldıb və ya stok kifayət etmir (məhsulun adı Message-dədir)
    }

    // Sifarişin nəticəsi: uğurlu olanda yaranan sifariş (sətirləri ilə) burdadır
    public class OrderPlaceResult
    {
        public OrderOutcome Outcome { get; set; }
        public string? Message { get; set; }
        public Order? Order { get; set; }

        public static OrderPlaceResult Of(OrderOutcome outcome, string? message = null)
        {
            return new OrderPlaceResult
            {
                Outcome = outcome,
                Message = message
            };
        }
    }

    // Servisin hazırladığı sifariş sorğusu (ünvan artıq yoxlanılıb): repository onu atomik icra edir
    public record OrderPlan(
        string UserId,
        string DeliveryFullName,
        string DeliveryPhone,
        string DeliveryLine,
        string DeliveryCity,
        string DeliveryPostalCode);
}
