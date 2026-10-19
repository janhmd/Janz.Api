using Domain.Entities;
using Domain.Enums;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IBaseRepository<CartItem> _cartItemRepo;
        private readonly IBaseRepository<Product> _productRepo;

        public OrderService(IOrderRepository orderRepo,
                            IBaseRepository<CartItem> cartItemRepo,
                            IBaseRepository<Product> productRepo)
        {
            _orderRepo = orderRepo;
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
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

        public async Task<ServiceResult<OrderDto>> CreateFromCartAsync(string userId)
        {
            var cartItems = (await _cartItemRepo.GetAllAsync()).Where(m => m.UserId == userId).ToList();
            if (cartItems.Count == 0)
            {
                return ServiceResult<OrderDto>.Fail("Cart is empty.");
            }

            var products = (await _productRepo.GetAllAsync()).ToDictionary(m => m.Id);
            foreach (var cartItem in cartItems)
            {
                if (!products.TryGetValue(cartItem.ProductId, out var product))
                {
                    return ServiceResult<OrderDto>.Fail("Product not found.");
                }

                if (cartItem.Quantity > product.Stock)
                {
                    return ServiceResult<OrderDto>.Fail("Not enough stock.");
                }
            }

            var orderItems = new List<OrderItem>();
            decimal total = 0;
            foreach (var cartItem in cartItems)
            {
                var product = products[cartItem.ProductId];
                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImagePath = product.ImagePath,
                    Price = product.Price,
                    Quantity = cartItem.Quantity
                });
                total += product.Price * cartItem.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                Total = total,
                Items = orderItems
            };

            await _orderRepo.AddAsync(order);

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepo.GetByIdAsync(cartItem.ProductId);
                if (product is not null)
                {
                    product.Stock -= cartItem.Quantity;
                    await _productRepo.UpdateAsync(product);
                }

                var tracked = await _cartItemRepo.GetByIdAsync(cartItem.Id);
                if (tracked is not null)
                {
                    await _cartItemRepo.DeleteAsync(tracked);
                }
            }

            return ServiceResult<OrderDto>.Ok(ToDto(order, order.Items));
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
