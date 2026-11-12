using back_end.Data;
using back_end.DTOs;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using Microsoft.EntityFrameworkCore;

namespace back_end.Services.Implements
{
    public class RevenueReportService : IRevenueReportService
    {
        private readonly DatabaseContext _context;
        private readonly ICurrentUserService _currentUser;

        public RevenueReportService(DatabaseContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<RevenueReportResponse> GetByPeriodAsync(string period, DateTime? startDate, DateTime? endDate, Guid? storeId)
        {
            string normalized = period.Trim().ToLowerInvariant();
            if (normalized is not ("day" or "week" or "month" or "quarter" or "year"))
                throw new ArgumentException("period must be day, week, month, quarter, or year.");

            var rows = await GetRowsAsync(startDate, endDate, storeId);
            var items = rows.GroupBy(row => PeriodStart(row.CompletedAt, normalized))
                .OrderBy(group => group.Key)
                .Select(group => new RevenueReportItem
                {
                    Key = group.Key.ToString("yyyy-MM-dd"),
                    Name = normalized,
                    PeriodStart = group.Key,
                    Revenue = group.Sum(row => row.Revenue),
                    OrderCount = group.Select(row => row.OrderId).Distinct().Count(),
                    QuantitySold = group.Sum(row => row.Quantity)
                }).ToList();

            return CreateResponse(rows, items, startDate, endDate, normalized);
        }

        public async Task<RevenueReportResponse> GetByStoreAsync(DateTime? startDate, DateTime? endDate, Guid? storeId)
        {
            var rows = await GetRowsAsync(startDate, endDate, storeId);
            List<Guid>? allowedStoreIds = storeId.HasValue ? new List<Guid> { storeId.Value } : await GetManagerStoreScopeAsync();
            var allocations = await GetStoreAllocationsAsync(rows, allowedStoreIds);
            var items = allocations.GroupBy(row => new { row.StoreId, row.StoreName })
                .OrderBy(group => group.Key.StoreName)
                .Select(group => new RevenueReportItem
                {
                    Key = group.Key.StoreId.ToString(), Name = group.Key.StoreName,
                    Revenue = group.Sum(row => row.Revenue),
                    OrderCount = group.Select(row => row.OrderId).Distinct().Count(),
                    QuantitySold = group.Sum(row => row.Quantity)
                }).ToList();
            return CreateResponse(rows, items, startDate, endDate);
        }

        public async Task<RevenueReportResponse> GetByProductAsync(DateTime? startDate, DateTime? endDate, Guid? storeId)
        {
            var rows = await GetRowsAsync(startDate, endDate, storeId);
            var items = rows.GroupBy(row => new { row.BookId, row.BookName })
                .OrderByDescending(group => group.Sum(row => row.Revenue))
                .Select(group => new RevenueReportItem
                {
                    Key = group.Key.BookId.ToString(), Name = group.Key.BookName,
                    Revenue = group.Sum(row => row.Revenue),
                    OrderCount = group.Select(row => row.OrderId).Distinct().Count(),
                    QuantitySold = group.Sum(row => row.Quantity)
                }).ToList();
            return CreateResponse(rows, items, startDate, endDate);
        }

        public async Task<RevenueReportResponse> GetByCategoryAsync(DateTime? startDate, DateTime? endDate, Guid? storeId)
        {
            var rows = await GetRowsAsync(startDate, endDate, storeId);
            var categories = await _context.BookCategories.AsNoTracking()
                .Where(link => rows.Select(row => row.BookId).Contains(link.BookId) &&
                    link.Category != null && !link.Category.IsDeleted)
                .Select(link => new { link.BookId, link.CategoryId, Name = link.Category!.Name })
                .ToListAsync();
            var items = categories.SelectMany(category => rows
                    .Where(row => row.BookId == category.BookId)
                    .Select(row => new { category.CategoryId, category.Name, Row = row }))
                .GroupBy(entry => new { entry.CategoryId, entry.Name })
                .OrderByDescending(group => group.Sum(entry => entry.Row.Revenue))
                .Select(group => new RevenueReportItem
                {
                    Key = group.Key.CategoryId.ToString(), Name = group.Key.Name,
                    Revenue = group.Sum(entry => entry.Row.Revenue),
                    OrderCount = group.Select(entry => entry.Row.OrderId).Distinct().Count(),
                    QuantitySold = group.Sum(entry => entry.Row.Quantity)
                }).ToList();
            return CreateResponse(rows, items, startDate, endDate);
        }

        private async Task<List<SaleRow>> GetRowsAsync(DateTime? startDate, DateTime? endDate, Guid? storeId)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
                throw new ArgumentException("startDate must be earlier than endDate; endDate is exclusive.");

            List<Guid>? managedStoreIds = null;
            if (_currentUser.GetRoleCurrentUser() == RoleUser.Manager)
            {
                Guid? userId = _currentUser.GetCurrentUserId();
                managedStoreIds = userId.HasValue
                    ? await _context.StoreStaffs.AsNoTracking()
                        .Where(staff => staff.UserId == userId.Value && !staff.IsDeleted &&
                            staff.User != null && staff.User.Enabled && !staff.User.IsDeleted &&
                            staff.Store != null && !staff.Store.IsDeleted)
                        .Select(staff => staff.StoreId).ToListAsync()
                    : new List<Guid>();
                if (storeId.HasValue && !managedStoreIds.Contains(storeId.Value))
                    throw new BusinessException(ErrorRecord.RevenueStoreAccessDenied);
            }

            var query = _context.OrderItems.AsNoTracking()
                .Where(item => item.Order != null && !item.Order.IsDeleted &&
                    item.Order.OrderStatus == OrderStatus.Completed &&
                    item.Order.PaymentStatus == PaymentStatus.Paid &&
                    item.Order.CompletedAt.HasValue && item.Book != null && !item.Book.IsDeleted);
            if (startDate.HasValue) query = query.Where(item => item.Order!.CompletedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(item => item.Order!.CompletedAt < endDate.Value);

            var data = await query.Select(item => new SaleRow
            {
                OrderId = item.OrderId,
                OrderCode = item.Order!.OrderCode,
                CompletedAt = item.Order.CompletedAt!.Value,
                BookId = item.BookId,
                BookName = item.Book!.Name,
                Quantity = item.Quantity,
                Revenue = item.TotalPrice
            }).ToListAsync();

            List<Guid>? targetStoreIds = storeId.HasValue ? new List<Guid> { storeId.Value } : managedStoreIds;
            if (targetStoreIds != null)
            {
                var codes = data.Select(row => row.OrderCode).Distinct().ToList();
                var reservations = await _context.InventoryTransactions.AsNoTracking()
                    .Where(transaction => transaction.Reason.StartsWith("Reserved for order ") &&
                        transaction.TransactionType == TransactionType.Adjustment && transaction.Quantity > 0 &&
                        !transaction.IsDeleted && transaction.Inventory != null && codes.Contains(transaction.Reason.Substring("Reserved for order ".Length)))
                    .Select(transaction => new
                    {
                        OrderCode = transaction.Reason.Substring("Reserved for order ".Length),
                        transaction.Inventory!.BookId, transaction.Inventory.StoreId, transaction.Quantity
                    }).ToListAsync();
                data = data.Select(row =>
                    {
                        var matching = reservations.Where(reservation => reservation.OrderCode == row.OrderCode && reservation.BookId == row.BookId).ToList();
                        int total = matching.Sum(reservation => reservation.Quantity);
                        int scoped = matching.Where(reservation => targetStoreIds.Contains(reservation.StoreId)).Sum(reservation => reservation.Quantity);
                        if (total == 0 || scoped == 0) return null;
                        row.Revenue = row.Revenue * scoped / total;
                        row.Quantity = scoped;
                        return row;
                    }).Where(row => row != null).Cast<SaleRow>().ToList();
            }
            return data;
        }

        private async Task<List<Guid>?> GetManagerStoreScopeAsync()
        {
            if (_currentUser.GetRoleCurrentUser() != RoleUser.Manager) return null;
            Guid? userId = _currentUser.GetCurrentUserId();
            if (!userId.HasValue) return new List<Guid>();
            return await _context.StoreStaffs.AsNoTracking()
                .Where(staff => staff.UserId == userId.Value && !staff.IsDeleted &&
                    staff.User != null && staff.User.Enabled && !staff.User.IsDeleted &&
                    staff.Store != null && !staff.Store.IsDeleted)
                .Select(staff => staff.StoreId).ToListAsync();
        }

        private async Task<List<StoreAllocation>> GetStoreAllocationsAsync(List<SaleRow> rows, List<Guid>? allowedStoreIds)
        {
            var codes = rows.Select(row => row.OrderCode).Distinct().ToList();
            var reservations = await _context.InventoryTransactions.AsNoTracking()
                .Where(transaction => codes.Contains(transaction.Reason.Replace("Reserved for order ", "")) &&
                    transaction.Reason.StartsWith("Reserved for order ") &&
                    transaction.TransactionType == TransactionType.Adjustment && transaction.Quantity > 0 &&
                    !transaction.IsDeleted && transaction.Inventory != null && transaction.Inventory.Store != null &&
                    !transaction.Inventory.Store.IsDeleted)
                .Select(transaction => new
                {
                    OrderCode = transaction.Reason.Substring("Reserved for order ".Length),
                    transaction.Inventory!.BookId,
                    transaction.Inventory.StoreId,
                    StoreName = transaction.Inventory.Store!.StoreCode,
                    transaction.Quantity
                }).ToListAsync();

            var allocations = new List<StoreAllocation>();
            foreach (var row in rows)
            {
                var matching = reservations.Where(reservation => reservation.OrderCode == row.OrderCode && reservation.BookId == row.BookId &&
                    (allowedStoreIds == null || allowedStoreIds.Contains(reservation.StoreId))).ToList();
                int totalReserved = matching.Sum(reservation => reservation.Quantity);
                if (totalReserved <= 0) continue;
                foreach (var group in matching.GroupBy(reservation => new { reservation.StoreId, reservation.StoreName }))
                {
                    int quantity = group.Sum(reservation => reservation.Quantity);
                    decimal revenue = row.Revenue * quantity / totalReserved;
                    allocations.Add(new StoreAllocation(row.OrderId, group.Key.StoreId, group.Key.StoreName, quantity, revenue));
                }
            }
            return allocations;
        }

        private static DateTime PeriodStart(DateTime date, string period) => period switch
        {
            "day" => date.Date,
            "week" => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
            "month" => new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind),
            "quarter" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1, 0, 0, 0, date.Kind),
            _ => new DateTime(date.Year, 1, 1, 0, 0, 0, date.Kind)
        };

        private static RevenueReportResponse CreateResponse(List<SaleRow> rows, List<RevenueReportItem> items,
            DateTime? startDate, DateTime? endDate, string? period = null) => new()
        {
            StartDate = startDate, EndDate = endDate, Period = period,
            TotalRevenue = rows.Sum(row => row.Revenue),
            OrderCount = rows.Select(row => row.OrderId).Distinct().Count(), Items = items
        };

        private sealed class SaleRow
        {
            public Guid OrderId { get; set; }
            public string OrderCode { get; set; } = string.Empty;
            public DateTime CompletedAt { get; set; }
            public Guid BookId { get; set; }
            public string BookName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Revenue { get; set; }
        }

        private sealed record StoreAllocation(Guid OrderId, Guid StoreId, string StoreName, int Quantity, decimal Revenue);
    }
}
