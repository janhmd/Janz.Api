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
                // Ləğv olunan sifarişin promo kodu yenidən istifadə oluna bilər
                var couponCode = await _dbSet.Where(m => m.Id == id).Select(m => m.CouponCode).FirstOrDefaultAsync();
                if (couponCode is not null)
                {
                    await _dbContext.Coupons
                            .Where(m => m.Code == couponCode && m.UsedCount > 0)
                            .ExecuteUpdateAsync(s => s.SetProperty(m => m.UsedCount, m => m.UsedCount - 1));
                }

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

                    // Variant hələ varsa onun stoku da qayıdır
                    if (item.VariantId is int variantId)
                    {
                        await _dbContext.ProductVariants
                                .Where(v => v.Id == variantId)
                                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock + item.Quantity));
                    }
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
                    .Include(m => m.Variant)
                        .ThenInclude(v => v!.Images)
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
                // Stok yalnız kifayət edirsə azalır (şərt bazada yoxlanılır): eyni anda iki sifariş son məhsulu ala bilməz.
                // Variant varsa əvvəl onun stoku, sonra məhsulun ümumi stoku (variantların cəmi) azalır
                var changed = 1;
                if (item.VariantId is int variantId)
                {
                    changed = await _dbContext.ProductVariants
                            .Where(v => v.Id == variantId && v.Stock >= item.Quantity)
                            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock - item.Quantity));
                }

                if (changed == 1)
                {
                    changed = await _dbContext.Products
                            .Where(p => p.Id == item.ProductId && p.Stock >= item.Quantity)
                            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Quantity));
                }

                if (changed == 0)
                {
                    await transaction.RollbackAsync();
                    _dbContext.ChangeTracker.Clear();
                    return OrderPlaceResult.Of(OrderOutcome.InsufficientStock, item.Variant is null ? item.Product.Name : $"{item.Product.Name} ({item.Variant.Label})");
                }

                var price = item.Variant?.Price ?? item.Product.Price;
                var image = item.Variant?.Images.OrderBy(m => m.Order).Select(m => m.ImagePath).FirstOrDefault() ?? item.Product.ImagePath;

                // Ad, şəkil və qiymət sifariş anındakı kimi saxlanılır (məhsul sonra dəyişsə də sifariş dəyişmir)
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ImagePath = image,
                    Price = price,
                    Quantity = item.Quantity,
                    VariantId = item.VariantId,
                    VariantLabel = item.Variant?.Label,
                    Grind = item.Grind
                });
                order.Total += price * item.Quantity;
            }

            if (plan.CouponCode is not null)
            {
                // Kupon yalnız indi də keçərlidirsə istifadə sayı artır (şərt bazada yoxlanılır: limit eyni anda keçilə bilməz)
                var today = DateTime.UtcNow.Date;
                var total = order.Total;
                var coupon = await _dbContext.Coupons.AsNoTracking().FirstOrDefaultAsync(m => m.Code == plan.CouponCode);

                // İlk sifariş kuponu: müştərinin ləğv olunmamış başqa sifarişi olmamalıdır
                var firstOrderTaken = coupon is not null && coupon.IsFirstOrder
                    && await _dbSet.AnyAsync(m => m.UserId == plan.UserId && m.Status != OrderStatus.Cancelled);

                var used = coupon is null || firstOrderTaken ? 0 : await _dbContext.Coupons
                        .Where(m => m.Id == coupon.Id && m.IsActive && m.MinOrder <= total
                            && (m.UsageLimit == null || m.UsedCount < m.UsageLimit)
                            && (m.StartsAt == null || m.StartsAt <= today)
                            && (m.EndsAt == null || m.EndsAt >= today))
                        .ExecuteUpdateAsync(s => s.SetProperty(m => m.UsedCount, m => m.UsedCount + 1));

                if (used == 0)
                {
                    await transaction.RollbackAsync();
                    _dbContext.ChangeTracker.Clear();
                    return OrderPlaceResult.Of(OrderOutcome.CouponInvalid);
                }

                // Endirim CouponService.CheckAsync-dəki kimi hesablanır: məbləğdən çox ola bilməz
                var discount = coupon!.Type == CouponType.Percent ? Math.Round(total * coupon.Value / 100, 2) : coupon.Value;
                order.CouponCode = coupon.Code;
                order.Discount = Math.Min(discount, total);
                order.Total = total - order.Discount;
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
