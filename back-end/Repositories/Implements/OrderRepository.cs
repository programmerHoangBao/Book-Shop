using back_end.Data;
using back_end.Entities;
using back_end.Enums;
using back_end.DTOs.Orders.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace back_end.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DatabaseContext _context;

        public OrderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<OrderSummaryResponse> GetSummaryAsync(List<Guid>? storeIds)
        {
            IQueryable<OrderEntity> query = _context.Orders.AsNoTracking()
                .Where(order => !order.IsDeleted);

            if (storeIds != null)
            {
                if (storeIds.Count == 0)
                    return new OrderSummaryResponse();

                query = query.Where(order => _context.InventoryTransactions.Any(transaction =>
                    !transaction.IsDeleted &&
                    transaction.TransactionType == TransactionType.Adjustment &&
                    transaction.Quantity > 0 &&
                    transaction.Reason == "Reserved for order " + order.OrderCode &&
                    transaction.Inventory != null && !transaction.Inventory.IsDeleted &&
                    storeIds.Contains(transaction.Inventory.StoreId)));
            }

            var counts = await query
                .GroupBy(order => order.OrderStatus)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();

            int Count(OrderStatus status) => counts
                .Where(item => item.Status == status)
                .Select(item => item.Count)
                .FirstOrDefault();

            return new OrderSummaryResponse
            {
                NewOrders = Count(OrderStatus.Pending),
                InProgressOrders = Count(OrderStatus.Confirmed) + Count(OrderStatus.Shipping),
                CompletedOrders = Count(OrderStatus.Completed),
                CancelledOrders = Count(OrderStatus.Cancelled)
            };
        }

        public async Task<(List<OrderEntity> Orders, int TotalItems)> GetInvoicesAsync(
            Guid userId,
            OrderStatus status,
            int pageNumber,
            int pageSize)
        {
            IQueryable<OrderEntity> query = _context.Orders
                .AsNoTracking()
                .Where(order => order.UserId == userId &&
                    order.OrderStatus == status &&
                    !order.IsDeleted);

            int totalItems = await query.CountAsync();
            List<OrderEntity> orders = await query
                .Include(order => order.Items)
                    .ThenInclude(item => item.Book)
                .OrderByDescending(order => order.OrderedAt)
                .ThenByDescending(order => order.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalItems);
        }

        public async Task<(CancelOrderResult Result, DateTime? CancelledAt)> CancelAsync(
            Guid orderId,
            Guid userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                OrderEntity? order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(candidate => candidate.Id == orderId &&
                        candidate.UserId == userId &&
                        !candidate.IsDeleted);
                if (order == null)
                {
                    await transaction.RollbackAsync();
                    return (CancelOrderResult.NotFound, null);
                }

                if (order.OrderStatus is not (OrderStatus.Pending or OrderStatus.Confirmed))
                {
                    await transaction.RollbackAsync();
                    return (CancelOrderResult.NotCancellable, null);
                }

                DateTime cancelledAt = DateTime.UtcNow;
                int updatedOrders = await _context.Orders
                    .Where(candidate => candidate.Id == orderId &&
                        candidate.UserId == userId &&
                        !candidate.IsDeleted &&
                        (candidate.OrderStatus == OrderStatus.Pending ||
                         candidate.OrderStatus == OrderStatus.Confirmed))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(candidate => candidate.OrderStatus, OrderStatus.Cancelled)
                        .SetProperty(candidate => candidate.CancelledAt, cancelledAt)
                        .SetProperty(candidate => candidate.UpdatedAt, cancelledAt));

                if (updatedOrders == 0)
                {
                    await transaction.RollbackAsync();
                    return (CancelOrderResult.NotCancellable, null);
                }

                string reservationReason = $"Reserved for order {order.OrderCode}";
                var reservations = await _context.InventoryTransactions
                    .Where(inventoryTransaction =>
                        inventoryTransaction.Reason == reservationReason &&
                        inventoryTransaction.TransactionType == TransactionType.Adjustment &&
                        !inventoryTransaction.IsDeleted)
                    .GroupBy(inventoryTransaction => inventoryTransaction.InventoryId)
                    .Select(group => new
                    {
                        InventoryId = group.Key,
                        Quantity = group.Sum(inventoryTransaction => inventoryTransaction.Quantity)
                    })
                    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    var inventory = await _context.Inventories
                        .Where(candidate => candidate.Id == reservation.InventoryId)
                        .Select(candidate => new
                        {
                            candidate.Quantity,
                            candidate.ReservedQuantity
                        })
                        .FirstOrDefaultAsync();
                    if (inventory == null)
                    {
                        throw new InvalidOperationException("Reserved inventory for the order was not found.");
                    }

                    int updatedInventory = await _context.Inventories
                        .Where(candidate => candidate.Id == reservation.InventoryId &&
                            candidate.ReservedQuantity >= reservation.Quantity)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(candidate => candidate.ReservedQuantity,
                                candidate => candidate.ReservedQuantity - reservation.Quantity)
                            .SetProperty(candidate => candidate.AvailableQuantity,
                                candidate => candidate.AvailableQuantity + reservation.Quantity)
                            .SetProperty(candidate => candidate.UpdatedAt, cancelledAt));
                    if (updatedInventory == 0)
                    {
                        throw new InvalidOperationException("Reserved inventory could not be released.");
                    }

                    _context.InventoryTransactions.Add(new InventoryTransactionEntity
                    {
                        InventoryId = reservation.InventoryId,
                        UserId = userId,
                        TransactionType = TransactionType.Adjustment,
                        Quantity = reservation.Quantity,
                        QuntityBefore = inventory.Quantity,
                        QuantityAfter = inventory.Quantity,
                        Reason = $"Released reservation for cancelled order {order.OrderCode}"
                    });
                }

                List<PromotionUsageEntity> promotionUsages = await _context.PromotionUsages
                    .Where(usage => usage.OrderId == orderId && !usage.IsDeleted)
                    .ToListAsync();
                foreach (PromotionUsageEntity usage in promotionUsages)
                {
                    int updatedPromotions = await _context.Promotions
                        .Where(promotion => promotion.Id == usage.PromotionId && promotion.UsageCount > 0)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(promotion => promotion.UsageCount,
                                promotion => promotion.UsageCount - 1)
                            .SetProperty(promotion => promotion.UpdatedAt, cancelledAt));
                    if (updatedPromotions == 0)
                    {
                        throw new InvalidOperationException("Promotion usage count could not be released.");
                    }

                    usage.IsDeleted = true;
                    usage.UpdatedAt = cancelledAt;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CancelOrderResult.Cancelled, cancelledAt);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(PlaceOrderResult Result, OrderEntity? Order)> PlaceOrderAsync(
            Guid userId,
            Guid addressId,
            PaymentMethod paymentMethod,
            Guid orderId,
            string? promotionCode)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                OrderEntity? duplicateOrder = await FindOrderAsync(orderId);
                if (duplicateOrder != null)
                {
                    await transaction.CommitAsync();
                    return duplicateOrder.UserId == userId
                        ? (PlaceOrderResult.Duplicate, duplicateOrder)
                        : (PlaceOrderResult.AddressNotFound, null);
                }

                bool addressExists = await _context.Addresses.AnyAsync(address =>
                    address.Id == addressId &&
                    address.UserId == userId &&
                    !address.IsDeleted);
                if (!addressExists)
                {
                    await transaction.RollbackAsync();
                    return (PlaceOrderResult.AddressNotFound, null);
                }

                CartEntity? cart = await _context.Carts.FirstOrDefaultAsync(candidate =>
                    candidate.UserId == userId && !candidate.IsDeleted);
                if (cart == null)
                {
                    await transaction.RollbackAsync();
                    return (PlaceOrderResult.CartEmpty, null);
                }

                List<CartItemEntity> cartItems = await _context.CartItems
                    .Include(item => item.Book)
                    .Where(item => item.CartId == cart.Id && !item.IsDeleted &&
                        item.Book != null && !item.Book.IsDeleted)
                    .ToListAsync();
                var orderLines = cartItems
                    .GroupBy(item => item.BookId)
                    .Select(group => new
                    {
                        Book = group.First().Book!,
                        Quantity = group.Sum(item => item.Quantity),
                        CartItems = group.ToList()
                    })
                    .OrderBy(line => line.Book.Id)
                    .ToList();

                if (orderLines.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return (PlaceOrderResult.CartEmpty, null);
                }

                if (orderLines.Any(line => line.Book.Status != BookStatus.Available))
                {
                    await transaction.RollbackAsync();
                    return (PlaceOrderResult.BookUnavailable, null);
                }

                DateTime now = DateTime.UtcNow;
                PromotionEntity? promotion = null;
                decimal discountAmount = 0;
                if (!string.IsNullOrWhiteSpace(promotionCode))
                {
                    string normalizedCode = promotionCode.Trim().ToUpperInvariant();
                    promotion = await _context.Promotions
                        .AsNoTracking()
                        .Include(candidate => candidate.PromotionBooks.Where(book => !book.IsDeleted))
                        .Include(candidate => candidate.PromotionCatatogies.Where(category => !category.IsDeleted))
                        .FirstOrDefaultAsync(candidate =>
                            candidate.Code != null && candidate.Code.ToUpper() == normalizedCode &&
                            candidate.IsActive && !candidate.IsDeleted &&
                            candidate.StartAt <= now && candidate.EndAt > now &&
                            (candidate.UsageLimit == 0 || candidate.UsageCount < candidate.UsageLimit));

                    if (promotion == null || orderLines.Sum(line => line.Book.Price * line.Quantity) < promotion.MinimumOrderValue)
                    {
                        await transaction.RollbackAsync();
                        return (PlaceOrderResult.InvalidPromotion, null);
                    }

                    List<Guid> promotionBookIds = promotion.PromotionBooks
                        .Select(target => target.BookId)
                        .ToList();
                    List<Guid> promotionCategoryIds = promotion.PromotionCatatogies
                        .Select(target => target.CategoryId)
                        .ToList();
                    bool hasTargetRestrictions = promotionBookIds.Count > 0 || promotionCategoryIds.Count > 0;
                    List<Guid> applicableBookIds = new();
                    List<Guid> orderBookIds = orderLines.Select(line => line.Book.Id).ToList();

                    if (!hasTargetRestrictions)
                    {
                        applicableBookIds = orderLines.Select(line => line.Book.Id).ToList();
                    }
                    else
                    {
                        HashSet<Guid> booksInPromotionCategories = promotionCategoryIds.Count == 0
                            ? new HashSet<Guid>()
                            : (await _context.BookCategories
                                .Where(category => !category.IsDeleted &&
                                    category.Category != null &&
                                    !category.Category.IsDeleted && category.Category.IsActive &&
                                    promotionCategoryIds.Contains(category.CategoryId) &&
                                    orderBookIds.Contains(category.BookId))
                                .Select(category => category.BookId)
                                .Distinct()
                                .ToListAsync())
                                .ToHashSet();

                        applicableBookIds = orderLines
                            .Where(line => promotionBookIds.Contains(line.Book.Id) ||
                                booksInPromotionCategories.Contains(line.Book.Id))
                            .Select(line => line.Book.Id)
                            .ToList();
                    }

                    decimal eligibleSubtotal = orderLines
                        .Where(line => applicableBookIds.Contains(line.Book.Id))
                        .Sum(line => line.Book.Price * line.Quantity);
                    if ((hasTargetRestrictions && eligibleSubtotal <= 0) ||
                        (promotion.Type != PromotionType.FreeShipping && eligibleSubtotal <= 0))
                    {
                        await transaction.RollbackAsync();
                        return (PlaceOrderResult.InvalidPromotion, null);
                    }

                    discountAmount = promotion.Type switch
                    {
                        PromotionType.PercentageDiscount => decimal.Round(
                            eligibleSubtotal * promotion.DiscountValue / 100m,
                            2,
                            MidpointRounding.AwayFromZero),
                        PromotionType.FixedAmountDiscount => Math.Min(
                            eligibleSubtotal,
                            promotion.DiscountValue),
                        PromotionType.FreeShipping => 0,
                        _ => 0
                    };
                }

                OrderEntity order = new()
                {
                    Id = orderId,
                    UserId = userId,
                    AddressId = addressId,
                    OrderCode = await GenerateOrderCodeAsync(),
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    OrderStatus = OrderStatus.Pending,
                    OrderedAt = DateTime.UtcNow
                };

                List<(Guid InventoryId, int Quantity, int PhysicalQuantity)> reservations = new();
                foreach (var line in orderLines)
                {
                    bool reserved = await TryReserveStockAsync(
                        line.Book.Id,
                        line.Quantity,
                        reservations);
                    if (!reserved)
                    {
                        await transaction.RollbackAsync();
                        return (PlaceOrderResult.InsufficientStock, null);
                    }

                    decimal lineTotal = line.Book.Price * line.Quantity;
                    order.Items.Add(new OrderItemEntity
                    {
                        OrderId = order.Id,
                        BookId = line.Book.Id,
                        Book = line.Book,
                        Quantity = line.Quantity,
                        UnitPrice = line.Book.Price,
                        DiscountAmount = 0,
                        TotalPrice = lineTotal
                    });
                    order.Subtotal += lineTotal;

                    foreach (CartItemEntity cartItem in line.CartItems)
                    {
                        cartItem.IsDeleted = true;
                    }
                }

                order.DiscountAmount = discountAmount;
                order.ShippingFee = 0;
                order.TotalAmount = Math.Max(0, order.Subtotal - order.DiscountAmount + order.ShippingFee);

                if (promotion != null)
                {
                    int updatedPromotions = await _context.Promotions
                        .Where(candidate => candidate.Id == promotion.Id &&
                            candidate.IsActive && !candidate.IsDeleted &&
                            candidate.StartAt <= now && candidate.EndAt > now &&
                            (candidate.UsageLimit == 0 || candidate.UsageCount < candidate.UsageLimit))
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(candidate => candidate.UsageCount,
                                candidate => candidate.UsageCount + 1)
                            .SetProperty(candidate => candidate.UpdatedAt, now));

                    if (updatedPromotions == 0)
                    {
                        await transaction.RollbackAsync();
                        return (PlaceOrderResult.InvalidPromotion, null);
                    }

                    _context.PromotionUsages.Add(new PromotionUsageEntity
                    {
                        PromotionId = promotion.Id,
                        UserId = userId,
                        OrderId = order.Id
                    });
                }

                foreach (var reservation in reservations)
                {
                    _context.InventoryTransactions.Add(new InventoryTransactionEntity
                    {
                        InventoryId = reservation.InventoryId,
                        UserId = userId,
                        TransactionType = TransactionType.Adjustment,
                        Quantity = reservation.Quantity,
                        QuntityBefore = reservation.PhysicalQuantity,
                        QuantityAfter = reservation.PhysicalQuantity,
                        Reason = $"Reserved for order {order.OrderCode}"
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (PlaceOrderResult.Created, order);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                OrderEntity? duplicateOrder = await FindOrderAsync(orderId);
                if (duplicateOrder?.UserId == userId)
                {
                    return (PlaceOrderResult.Duplicate, duplicateOrder);
                }
                throw;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(ConfirmOrderResult Result, OrderEntity? Order)> ConfirmAsync(
            Guid orderId,
            Guid? storeId)
        {
            OrderEntity? order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order == null)
            {
                return (ConfirmOrderResult.NotFound, null);
            }

            if (storeId.HasValue)
            {
                string reservationReason = $"Reserved for order {order.OrderCode}";
                bool orderUsesStore = await _context.InventoryTransactions.AnyAsync(transaction =>
                    transaction.Reason == reservationReason &&
                    !transaction.IsDeleted &&
                    transaction.Inventory != null &&
                    transaction.Inventory.StoreId == storeId.Value);
                if (!orderUsesStore)
                {
                    return (ConfirmOrderResult.StoreNotAssociated, null);
                }
            }

            if (order.OrderStatus == OrderStatus.Confirmed)
            {
                return (ConfirmOrderResult.AlreadyConfirmed, order);
            }
            if (order.OrderStatus != OrderStatus.Pending)
            {
                return (ConfirmOrderResult.NotPending, order);
            }

            DateTime confirmedAt = DateTime.UtcNow;
            int updatedRows = await _context.Orders
                .Where(candidate => candidate.Id == orderId &&
                    !candidate.IsDeleted &&
                    candidate.OrderStatus == OrderStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.OrderStatus, OrderStatus.Confirmed)
                    .SetProperty(candidate => candidate.UpdatedAt, confirmedAt));

            if (updatedRows == 1)
            {
                order.OrderStatus = OrderStatus.Confirmed;
                order.UpdatedAt = confirmedAt;
                return (ConfirmOrderResult.Confirmed, order);
            }

            order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order?.OrderStatus == OrderStatus.Confirmed)
            {
                return (ConfirmOrderResult.AlreadyConfirmed, order);
            }

            return order == null
                ? (ConfirmOrderResult.NotFound, null)
                : (ConfirmOrderResult.NotPending, order);
        }

        public async Task<(SetOrderInTransitResult Result, OrderEntity? Order)> SetInTransitAsync(
            Guid orderId,
            Guid? storeId)
        {
            OrderEntity? order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order == null)
            {
                return (SetOrderInTransitResult.NotFound, null);
            }

            if (storeId.HasValue && !await OrderUsesStoreAsync(order, storeId.Value))
            {
                return (SetOrderInTransitResult.StoreNotAssociated, null);
            }

            if (order.OrderStatus == OrderStatus.Shipping)
            {
                return (SetOrderInTransitResult.AlreadyInTransit, order);
            }
            if (order.OrderStatus != OrderStatus.Confirmed)
            {
                return (SetOrderInTransitResult.NotConfirmed, order);
            }

            DateTime inTransitAt = DateTime.UtcNow;
            int updatedRows = await _context.Orders
                .Where(candidate => candidate.Id == orderId &&
                    !candidate.IsDeleted &&
                    candidate.OrderStatus == OrderStatus.Confirmed)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.OrderStatus, OrderStatus.Shipping)
                    .SetProperty(candidate => candidate.UpdatedAt, inTransitAt));

            if (updatedRows == 1)
            {
                order.OrderStatus = OrderStatus.Shipping;
                order.UpdatedAt = inTransitAt;
                return (SetOrderInTransitResult.InTransit, order);
            }

            order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order?.OrderStatus == OrderStatus.Shipping)
            {
                return (SetOrderInTransitResult.AlreadyInTransit, order);
            }

            return order == null
                ? (SetOrderInTransitResult.NotFound, null)
                : (SetOrderInTransitResult.NotConfirmed, order);
        }

        public async Task<(CompleteOrderResult Result, OrderEntity? Order)> CompleteAsync(
            Guid orderId,
            Guid? storeId)
        {
            OrderEntity? order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order == null)
            {
                return (CompleteOrderResult.NotFound, null);
            }

            if (storeId.HasValue && !await OrderUsesStoreAsync(order, storeId.Value))
            {
                return (CompleteOrderResult.StoreNotAssociated, null);
            }

            if (order.OrderStatus == OrderStatus.Completed)
            {
                return (CompleteOrderResult.AlreadyCompleted, order);
            }
            if (order.OrderStatus != OrderStatus.Shipping)
            {
                return (CompleteOrderResult.NotInTransit, order);
            }

            DateTime completedAt = DateTime.UtcNow;
            int updatedRows = await _context.Orders
                .Where(candidate => candidate.Id == orderId &&
                    !candidate.IsDeleted &&
                    candidate.OrderStatus == OrderStatus.Shipping)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.OrderStatus, OrderStatus.Completed)
                    .SetProperty(candidate => candidate.PaymentStatus, PaymentStatus.Paid)
                    .SetProperty(candidate => candidate.CompletedAt, completedAt)
                    .SetProperty(candidate => candidate.UpdatedAt, completedAt));

            if (updatedRows == 1)
            {
                order.OrderStatus = OrderStatus.Completed;
                order.PaymentStatus = PaymentStatus.Paid;
                order.CompletedAt = completedAt;
                order.UpdatedAt = completedAt;
                return (CompleteOrderResult.Completed, order);
            }

            order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == orderId && !candidate.IsDeleted);
            if (order?.OrderStatus == OrderStatus.Completed)
            {
                return (CompleteOrderResult.AlreadyCompleted, order);
            }

            return order == null
                ? (CompleteOrderResult.NotFound, null)
                : (CompleteOrderResult.NotInTransit, order);
        }

        private Task<bool> OrderUsesStoreAsync(OrderEntity order, Guid storeId)
        {
            string reservationReason = $"Reserved for order {order.OrderCode}";
            return _context.InventoryTransactions.AnyAsync(transaction =>
                transaction.Reason == reservationReason &&
                !transaction.IsDeleted &&
                transaction.Inventory != null &&
                transaction.Inventory.StoreId == storeId);
        }

        private Task<OrderEntity?> FindOrderAsync(Guid orderId) =>
            _context.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == orderId && !order.IsDeleted);

        private async Task<bool> TryReserveStockAsync(
            Guid bookId,
            int quantity,
            List<(Guid InventoryId, int Quantity, int PhysicalQuantity)> reservations)
        {
            int remaining = quantity;
            List<Guid> inventoryIds = await _context.Inventories
                .Where(inventory => inventory.BookId == bookId &&
                    !inventory.IsDeleted &&
                    inventory.Store != null &&
                    !inventory.Store.IsDeleted &&
                    inventory.AvailableQuantity > 0)
                .OrderByDescending(inventory => inventory.AvailableQuantity)
                .ThenBy(inventory => inventory.Id)
                .Select(inventory => inventory.Id)
                .ToListAsync();

            foreach (Guid inventoryId in inventoryIds)
            {
                while (remaining > 0)
                {
                    var stock = await _context.Inventories
                        .Where(inventory => inventory.Id == inventoryId && !inventory.IsDeleted)
                        .Select(inventory => new
                        {
                            inventory.AvailableQuantity,
                            inventory.Quantity
                        })
                        .FirstOrDefaultAsync();

                    if (stock == null || stock.AvailableQuantity <= 0)
                    {
                        break;
                    }

                    int reserveQuantity = Math.Min(remaining, stock.AvailableQuantity);
                    int updated = await _context.Inventories
                        .Where(inventory => inventory.Id == inventoryId &&
                            !inventory.IsDeleted &&
                            inventory.AvailableQuantity >= reserveQuantity)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(inventory => inventory.AvailableQuantity,
                                inventory => inventory.AvailableQuantity - reserveQuantity)
                            .SetProperty(inventory => inventory.ReservedQuantity,
                                inventory => inventory.ReservedQuantity + reserveQuantity));

                    if (updated == 0)
                    {
                        continue;
                    }

                    reservations.Add((inventoryId, reserveQuantity, stock.Quantity));
                    remaining -= reserveQuantity;
                }

                if (remaining == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                string orderCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
                if (!await _context.Orders.AnyAsync(order => order.OrderCode == orderCode))
                {
                    return orderCode;
                }
            }

            throw new InvalidOperationException("Could not generate a unique order code.");
        }
    }
}
