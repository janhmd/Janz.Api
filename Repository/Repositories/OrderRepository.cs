using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;
using Repository.Results;
using Domain.Enums;

namespace Repository.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Order>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<Order?> GetByIdForUserAsync(int id, string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }
    

        public async Task<IReadOnlyList<Order>> GetAllForAdminAsync(OrderStatus? status)
        {
            var query = _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .Include(m => m.User)
                    .AsQueryable();

            if (status is not null)
            {
                query = query.Where(m => m.Status == status);
            }

            return await query
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<Order?> GetByIdForAdminAsync(int id)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Items)
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> ChangeStatusAsync(int id, OrderStatus from, OrderStatus to)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Şərt bazada yoxlanılır: sifariş artıq başqa statusa keçibsə heç nə dəyişmir
            var changed = await _dbSet
                    .Where(m => m.Id == id && m.Status == from)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.Status, to));

            if (changed == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            if (to == OrderStatus.Cancelled)
            {
                // Ləğv olunan sifarişin məhsulları stoka qayıdır
                var items = await _dbContext.OrderItems
                        .AsNoTracking()
                        .Where(m => m.OrderId == id)
                        .ToListAsync();

                foreach (var item in items)
                {
                    await _dbContext.Products
                            .Where(p => p.Id == item.ProductId)
                            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
                }
            }

            await transaction.CommitAsync();
            return true;
        }

        public async Task<OrderPlaceResult> PlaceAsync(OrderPlan plan)
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Səbət bu tranzaksiyada təzədən oxunur: servisdən gələn rəqəmlərə güvənmirik
            var cartItems = await _dbContext.CartItems
                    .Include(m => m.Product)
                    .Where(m => m.UserId == plan.UserId)
                    .ToListAsync();

            if (cartItems.Count == 0)
            {
                return OrderPlaceResult.Of(OrderOutcome.CartEmpty);
            }

            var order = new Order
            {
                UserId = plan.UserId,
                Status = OrderStatus.Pending,
                DeliveryFullName = plan.DeliveryFullName,
                DeliveryPhone = plan.DeliveryPhone,
                DeliveryLine = plan.DeliveryLine,
                DeliveryCity = plan.DeliveryCity,
                DeliveryPostalCode = plan.DeliveryPostalCode
            };

            foreach (var item in cartItems)
            {
                // Stok yalnız kifayət edirsə azalır (şərt bazada yoxlanılır): eyni anda iki sifariş son məhsulu ala bilməz
                var changed = await _dbContext.Products
                        .Where(p => p.Id == item.ProductId && p.Stock >= item.Quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Quantity));

                if (changed == 0)
                {
                    await transaction.RollbackAsync();
                    _dbContext.ChangeTracker.Clear();
                    return OrderPlaceResult.Of(OrderOutcome.InsufficientStock, item.Product.Name);
                }

                // Ad, şəkil və qiymət sifariş anındakı kimi saxlanılır (məhsul sonra dəyişsə də sifariş dəyişmir)
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ImagePath = item.Product.ImagePath,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                });
                order.Total += item.Product.Price * item.Quantity;
            }

            _dbContext.Orders.Add(order);
            _dbContext.CartItems.RemoveRange(cartItems);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OrderPlaceResult
            {
                Outcome = OrderOutcome.Success,
                Order = order
            };
        }
    }
}
