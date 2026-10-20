using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Repository.Results;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IAddressRepository _addressRepo;
        private readonly IValidator<CreateOrderDto> _createValidator;

        public OrderService(IOrderRepository orderRepo,
                            IAddressRepository addressRepo,
                            IValidator<CreateOrderDto> createValidator)
        {
            _orderRepo = orderRepo;
            _addressRepo = addressRepo;
            _createValidator = createValidator;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync(string userId)
        {
            // Sətirlər Include ilə birlikdə gəlir: bütün OrderItems cədvəlini yükləməyə ehtiyac yoxdur
            var orders = await _orderRepo.GetByUserAsync(userId);
            return orders.Select(m => ToDto(m, m.Items));
        }

        public async Task<ServiceResult<OrderDto>> GetByIdAsync(string userId, int id)
        {
            var order = await _orderRepo.GetByIdForUserAsync(id, userId);
            if (order is null)
            {
                return ServiceResult<OrderDto>.NotFound();
            }

            return ServiceResult<OrderDto>.Ok(ToDto(order, order.Items));
        }

        public async Task<ServiceResult<OrderDto>> CreateFromCartAsync(string userId, CreateOrderDto model)
        {
            var validation = await _createValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<OrderDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            // Ünvan yalnız bu istifadəçiyə aiddirsə qəbul olunur (başqasının ünvanına sifariş göndərilə bilməz)
            var address = await _addressRepo.GetByIdForUserAsync(model.AddressId, userId);
            if (address is null)
            {
                return ServiceResult<OrderDto>.Fail("Choose a delivery address.");
            }

            var plan = new OrderPlan(userId, address.FullName, address.Phone, address.Line, address.City, address.PostalCode);

            // Stok, sifariş və səbət bir tranzaksiyada dəyişir (OrderRepository.PlaceAsync)
            var result = await _orderRepo.PlaceAsync(plan);

            return result.Outcome switch
            {
                OrderOutcome.Success => ServiceResult<OrderDto>.Ok(ToDto(result.Order!, result.Order!.Items)),
                OrderOutcome.CartEmpty => ServiceResult<OrderDto>.Fail("Cart is empty."),
                OrderOutcome.InsufficientStock => ServiceResult<OrderDto>.Fail($"Not enough stock for {result.Message}."),
                _ => ServiceResult<OrderDto>.Fail("The order could not be placed.")
            };
        }

        private static OrderDto ToDto(Order order, IEnumerable<OrderItem> items)
        {
            var dto = new OrderDto();
            Fill(dto, order, items);
            return dto;
        }

        private static void Fill(OrderDto dto, Order order, IEnumerable<OrderItem> items)
        {
            dto.Id = order.Id;
            dto.Status = order.Status.ToString();
            dto.Total = order.Total;
            dto.CreatedAt = order.CreatedAt;
            dto.DeliveryFullName = order.DeliveryFullName;
            dto.DeliveryPhone = order.DeliveryPhone;
            dto.DeliveryLine = order.DeliveryLine;
            dto.DeliveryCity = order.DeliveryCity;
            dto.DeliveryPostalCode = order.DeliveryPostalCode;
            dto.Items = items.Select(ToDto).ToList();
        }

        private static OrderItemDto ToDto(OrderItem item)
        {
            var dto = new OrderItemDto();
            Fill(dto, item);
            return dto;
        }

        private static void Fill(OrderItemDto dto, OrderItem item)
        {
            dto.Id = item.Id;
            dto.ProductId = item.ProductId;
            dto.ProductName = item.ProductName;
            dto.ImagePath = item.ImagePath;
            dto.Price = item.Price;
            dto.Quantity = item.Quantity;
            dto.Total = item.Price * item.Quantity;
        }
    }
}
