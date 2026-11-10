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

                // Balansdan ödənən hissə balansa qaytarılır
                var paidFromBalance = await _dbSet.Where(m => m.Id == id).Select(m => new { m.UserId, m.BalanceUsed }).FirstAsync();
                if (paidFromBalance.BalanceUsed > 0)
                {
                    _dbContext.BalanceTransactions.Add(new BalanceTransaction
                    {
                        UserId = paidFromBalance.UserId,
                        Amount = paidFromBalance.BalanceUsed,
                        Type = BalanceTransactionType.Refund,
                        Note = $"Refund for cancelled order #{id}",
                        OrderId = id,
                        IsConfirmed = true
                    });
                    await _dbContext.SaveChangesAsync();
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

        public async Task<Order?> GetAnyByIdAsync(int id)
        {
            return await _dbSet
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IReadOnlyList<Order>> GetAwaitingAsync(string? userId, DateTime? createdBefore)
        {
            var query = _dbSet
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(m => m.IsAwaitingPayment);

            if (userId is not null)
            {
                query = query.Where(m => m.UserId == userId);
            }

            if (createdBefore is not null)
            {
                query = query.Where(m => m.CreatedAt < createdBefore);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> FinalizePaidAsync(int id)
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Şərt bazada yoxlanılır: sifariş artıq həqiqi olubsa (ya da silinibsə) heç nə dəyişmir
            var changed = await _dbSet
                    .IgnoreQueryFilters()
                    .Where(m => m.Id == id && m.IsAwaitingPayment)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.IsAwaitingPayment, false)
                        .SetProperty(m => m.IsPaid, true)
                        .SetProperty(m => m.PaidAt, DateTime.UtcNow));

            if (changed == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Sifarişə düşən məhsullar səbətdən çıxır (ödəniş vaxtı səbətə əlavə olunanlar qalır)
            var order = await _dbSet.AsNoTracking().Include(m => m.Items).FirstAsync(m => m.Id == id);
            var cartItems = await _dbContext.CartItems.Where(m => m.UserId == order.UserId).ToListAsync();
            var ordered = cartItems
                    .Where(c => order.Items.Any(i => i.ProductId == c.ProductId && i.VariantId == c.VariantId))
                    .ToList();
            _dbContext.CartItems.RemoveRange(ordered);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> DiscardAsync(int id)
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var order = await _dbSet
                    .IgnoreQueryFilters()
                    .Include(m => m.Items)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsAwaitingPayment && !m.IsPaid);

            if (order is null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Ayrılmış stok qayıdır
            foreach (var item in order.Items)
            {
                await _dbContext.Products
                        .Where(p => p.Id == item.ProductId)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));

                if (item.VariantId is int variantId)
                {
                    await _dbContext.ProductVariants
                            .Where(v => v.Id == variantId)
                            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock + item.Quantity));
                }
            }

            // Promo kod yenidən istifadə oluna bilər
            if (order.CouponCode is not null)
            {
                await _dbContext.Coupons
                        .Where(m => m.Code == order.CouponCode && m.UsedCount > 0)
                        .ExecuteUpdateAsync(s => s.SetProperty(m => m.UsedCount, m => m.UsedCount - 1));
            }

            // Balansdan tutulan pul qayıdır: sifarişə yazılan hərəkət silinir, tarixçədə iz qalmır
            await _dbContext.BalanceTransactions
                    .Where(m => m.OrderId == id && m.Type == BalanceTransactionType.Order)
                    .ExecuteDeleteAsync();

            // Sifariş və sətirləri tam silinir (sətirlər cascade ilə)
            _dbSet.Remove(order);
            await _dbContext.SaveChangesAsync();

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

            // Səviyyə endirimi: çatdırılmış sifarişlərə xərclənən məbləğə görə (DiscountLevelService-dəki kimi)
            var total = order.Total;
            var spent = await _dbSet
                    .Where(m => m.UserId == plan.UserId && m.Status == OrderStatus.Delivered)
                    .SumAsync(m => m.Total);
            var level = await _dbContext.DiscountLevels
                    .AsNoTracking()
                    .Where(m => m.MinSpent <= spent)
                    .OrderByDescending(m => m.MinSpent)
                    .FirstOrDefaultAsync();
            var levelDiscount = level is null ? 0 : Math.Round(total * level.Percent / 100, 2);

            if (plan.CouponCode is not null)
            {
                var today = DateTime.UtcNow.Date;
                var coupon = await _dbContext.Coupons.AsNoTracking().FirstOrDefaultAsync(m => m.Code == plan.CouponCode);

                // Endirim CouponService.CheckAsync-dəki kimi hesablanır: məbləğdən çox ola bilməz
                var couponDiscount = coupon is null ? 0
                    : Math.Min(coupon.Type == CouponType.Percent ? Math.Round(total * coupon.Value / 100, 2) : coupon.Value, total);

                // Kupon və səviyyə endirimi toplanmır: böyüyü tətbiq olunur. Səviyyə böyükdürsə kupon işlədilmir
                if (coupon is null || couponDiscount > levelDiscount)
                {
                    // İlk sifariş kuponu: müştərinin ləğv olunmamış başqa sifarişi olmamalıdır
                    var firstOrderTaken = coupon is not null && coupon.IsFirstOrder
                        && await _dbSet.AnyAsync(m => m.UserId == plan.UserId && m.Status != OrderStatus.Cancelled);

                    // Kupon yalnız indi də keçərlidirsə istifadə sayı artır (şərt bazada yoxlanılır: limit eyni anda keçilə bilməz)
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

                    order.CouponCode = coupon!.Code;
                    order.Discount = couponDiscount;
                }
            }

            if (order.CouponCode is null && levelDiscount > 0)
            {
                order.LevelName = level!.Name;
                order.Discount = levelDiscount;
            }

            order.Total = total - order.Discount;

            // Çatdırılma haqqı endirimdən sonrakı məbləğə görə: müəyyən həddən yuxarı pulsuz
            order.DeliveryFee = plan.FreeDeliveryFrom > 0 && order.Total >= plan.FreeDeliveryFrom ? 0 : plan.DeliveryFee;
            order.Total += order.DeliveryFee;

            // Balansdan ödəniş: balans bu tranzaksiyada kilidlə oxunur ki, eyni pul iki sifarişə xərclənməsin
            if (plan.UseBalance && order.Total > 0)
            {
                var balance = (await _dbContext.BalanceTransactions
                        .FromSqlInterpolated($"SELECT * FROM BalanceTransactions WITH (UPDLOCK, HOLDLOCK) WHERE UserId = {plan.UserId} AND IsConfirmed = 1")
                        .Select(m => m.Amount)
                        .ToListAsync())
                        .Sum();

                order.BalanceUsed = Math.Min(Math.Max(balance, 0), order.Total);
                if (order.BalanceUsed == order.Total)
                {
                    order.IsPaid = true;
                    order.PaidAt = DateTime.UtcNow;
                }
            }

            // Kartla ödəniləcək hissə qalırsa pul alınana qədər sifariş "gözləyir": səbət toxunulmur ki, ödəniş alınmasa
            // müştəri məhsulları səbətdə tapıb yenidən ödəsin
            order.IsAwaitingPayment = plan.AwaitCard && !order.IsPaid;

            _dbContext.Orders.Add(order);
            if (!order.IsAwaitingPayment)
            {
                _dbContext.CartItems.RemoveRange(cartItems);
            }

            await _dbContext.SaveChangesAsync();

            if (order.BalanceUsed > 0)
            {
                _dbContext.BalanceTransactions.Add(new BalanceTransaction
                {
                    UserId = plan.UserId,
                    Amount = -order.BalanceUsed,
                    Type = BalanceTransactionType.Order,
                    Note = $"Payment for order #{order.Id}",
                    OrderId = order.Id,
                    IsConfirmed = true
                });
                await _dbContext.SaveChangesAsync();
            }
            await transaction.CommitAsync();

            return new OrderPlaceResult
            {
                Outcome = OrderOutcome.Success,
                Order = order
            };
        }
    }
}
