using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Repository.Results;
using Service.Helpers;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.DTOs.Refunds;
using Service.Helpers.Exceptions;
using Service.Helpers.Responses;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IAddressRepository _addressRepo;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<UpdateOrderStatusDto> _statusValidator;
        private readonly INotificationService _notificationService;
        private readonly IRefundRepository _refundRepo;

        public OrderService(IOrderRepository orderRepo,
                            IAddressRepository addressRepo,
                            IValidator<CreateOrderDto> createValidator,
                            IValidator<UpdateOrderStatusDto> statusValidator,
                            INotificationService notificationService,
                            IRefundRepository refundRepo)
        {
            _orderRepo = orderRepo;
            _addressRepo = addressRepo;
            _createValidator = createValidator;
            _statusValidator = statusValidator;
            _notificationService = notificationService;
            _refundRepo = refundRepo;
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

            var dto = ToDto(order, order.Items);
            await FillRefundAsync(dto, order);
            return ServiceResult<OrderDto>.Ok(dto);
        }

        // Sifariş səhifəsində pulu geri qaytarma bölməsi: ən son sorğu və yeni sorğu göndərmək olarmı
        private async Task FillRefundAsync(OrderDto dto, Order order)
        {
            var latest = await _refundRepo.GetLatestByOrderAsync(order.Id);
            dto.Refund = latest is null ? null : RefundService.ToDto(latest);
            dto.CanRequestRefund = RefundService.CannotRequestReason(order, latest) is null;
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

            var couponCode = string.IsNullOrWhiteSpace(model.CouponCode) ? null : model.CouponCode.Trim().ToUpperInvariant();
            var plan = new OrderPlan(userId, address.FullName, address.Phone, address.Line, address.City, address.PostalCode, couponCode, model.UseBalance);

            // Stok, sifariş və səbət bir tranzaksiyada dəyişir (OrderRepository.PlaceAsync)
            var result = await _orderRepo.PlaceAsync(plan);

            if (result.Outcome == OrderOutcome.Success)
            {
                await _notificationService.AddAsync(userId, NotificationKind.Shop, $"Order #{result.Order!.Id} was placed",
                    $"We received your order for ₼{result.Order.Total:0.00}.", $"/Orders/Detail/{result.Order.Id}");
            }

            return result.Outcome switch
            {
                OrderOutcome.Success => ServiceResult<OrderDto>.Ok(ToDto(result.Order!, result.Order!.Items)),
                OrderOutcome.CartEmpty => ServiceResult<OrderDto>.Fail("Cart is empty."),
                OrderOutcome.InsufficientStock => ServiceResult<OrderDto>.Fail($"Not enough stock for {result.Message}."),
                OrderOutcome.CouponInvalid => ServiceResult<OrderDto>.Fail("This promo code is not valid for this order."),
                _ => ServiceResult<OrderDto>.Fail("The order could not be placed.")
            };
        }

        public async Task<IEnumerable<AdminOrderDto>> GetAllForAdminAsync(string? status, string? search)
        {
            OrderStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<OrderStatus>(status, true, out var parsed) || int.TryParse(status, out _))
                {
                    throw new BadRequestException("This status is not known.");
                }
                filter = parsed;
            }

            var orders = await _orderRepo.GetAllForAdminAsync(filter);

            // Axtarış: "#12" yalnız sifariş nömrəsinə baxır; qalan mətn müştərinin adı, emaili və ya alıcının adı/telefonu ilədir
            if (!string.IsNullOrWhiteSpace(search) && search.Trim().StartsWith('#'))
            {
                var number = search.Trim().TrimStart('#');
                orders = orders.Where(m => m.Id.ToString() == number).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                orders = orders.Where(m =>
                        m.Id.ToString() == text ||
                        (m.User.Email ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        (m.User.Name + " " + m.User.Surname).Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        m.DeliveryFullName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        m.DeliveryPhone.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return orders.Select(ToAdminDto);
        }

        public async Task<AdminOrderDto> GetByIdForAdminAsync(int id)
        {
            var order = await _orderRepo.GetByIdForAdminAsync(id);
            if (order is null)
            {
                throw new NotFoundException();
            }

            var dto = ToAdminDto(order);
            await FillRefundAsync(dto, order);
            return dto;
        }

        public async Task ChangeStatusAsync(int id, UpdateOrderStatusDto model)
        {
            await _statusValidator.EnsureValidAsync(model);
            var to = Enum.Parse<OrderStatus>(model.Status!, true);

            var order = await _orderRepo.GetByIdForAdminAsync(id);
            if (order is null)
            {
                throw new NotFoundException();
            }

            // Yalnız icazə verilən addım: Pending -> Processing -> Shipped -> Delivered, ləğv göndərilməmiş sifarişdə
            if (!OrderStatusFlow.CanMove(order.Status, to))
            {
                throw new BadRequestException($"The order can't move from {order.Status} to {to}.");
            }

            if (!await _orderRepo.ChangeStatusAsync(id, order.Status, to))
            {
                throw new BadRequestException("The order status was changed by someone else. Refresh the page.");
            }

            await _notificationService.AddAsync(order.UserId, NotificationKind.Shop, $"Order #{order.Id} is now {to}",
                StatusText(to), $"/Orders/Detail/{order.Id}");
        }

        private static string StatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Processing => "We are preparing your order.",
                OrderStatus.Shipped => "Your order is on the way.",
                OrderStatus.Delivered => "Your order was delivered. Enjoy!",
                OrderStatus.Cancelled => "Your order was cancelled. Money paid from the balance is returned to it.",
                _ => "The order status was updated."
            };
        }

        // Dashboard-dakı "son sifarişlər" də eyni formada göstərilir
        public static AdminOrderDto ToAdminDto(Order order)
        {
            var dto = new AdminOrderDto();
            Fill(dto, order, order.Items);
            dto.UserId = order.UserId;
            dto.CustomerName = (order.User.Name + " " + order.User.Surname).Trim();
            dto.CustomerEmail = order.User.Email ?? string.Empty;
            dto.ItemCount = order.Items.Sum(m => m.Quantity);
            dto.NextStatuses = OrderStatusFlow.NextOf(order.Status).Select(m => m.ToString()).ToList();
            return dto;
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
            dto.CouponCode = order.CouponCode;
            dto.LevelName = order.LevelName;
            dto.Discount = order.Discount;
            dto.BalanceUsed = order.BalanceUsed;
            dto.IsPaid = order.IsPaid;
            dto.PaidAt = order.PaidAt;
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
            dto.VariantLabel = item.VariantLabel;
            dto.Grind = item.Grind;
        }
    }
}
