using back_end.Data;
using back_end.DTOs;
using back_end.DTOs.Merchandise;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back_end.Services.Implements
{
    public class MerchandiseReportService : IMerchandiseReportService
    {
        private readonly DatabaseContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStoreStaffRepository _storeStaffRepository;

        public MerchandiseReportService(
            DatabaseContext context,
            ICurrentUserService currentUser,
            IStoreStaffRepository storeStaffRepository)
        {
            _context = context;
            _currentUser = currentUser;
            _storeStaffRepository = storeStaffRepository;
        }

        public async Task<ApiResponse<List<MerchandiseBookReportItem>>> GetLowStockAsync(int threshold)
        {
            if (threshold < 0 || threshold > 1_000_000)
                throw new BusinessException(ErrorRecord.RequestInvalid);

            List<Guid>? storeIds = await GetStoreScopeAsync();
            var inventory = GetInventoryQuery(storeIds);
            var rows = await inventory
                .GroupBy(item => new { item.BookId, item.Book!.Name, item.Book.Author })
                .Select(group => new MerchandiseBookReportItem
                {
                    BookId = group.Key.BookId,
                    Name = group.Key.Name,
                    Author = group.Key.Author,
                    AvailableQuantity = group.Sum(item => item.AvailableQuantity)
                })
                .Where(item => item.AvailableQuantity > 0 && item.AvailableQuantity <= threshold)
                .OrderBy(item => item.AvailableQuantity)
                .ThenBy(item => item.Name)
                .ToListAsync();

            return Success(rows);
        }

        public async Task<ApiResponse<List<MerchandiseBookReportItem>>> GetOutOfStockAsync()
        {
            List<Guid>? storeIds = await GetStoreScopeAsync();
            var rows = await GetInventoryQuery(storeIds)
                .GroupBy(item => new { item.BookId, item.Book!.Name, item.Book.Author })
                .Select(group => new MerchandiseBookReportItem
                {
                    BookId = group.Key.BookId,
                    Name = group.Key.Name,
                    Author = group.Key.Author,
                    AvailableQuantity = group.Sum(item => item.AvailableQuantity)
                })
                .Where(item => item.AvailableQuantity <= 0)
                .OrderBy(item => item.Name)
                .ToListAsync();

            return Success(rows);
        }

        public async Task<ApiResponse<List<MerchandiseBookReportItem>>> GetBestSellingAsync(int periodDays, int limit)
        {
            ValidateSalesParameters(periodDays);
            if (limit < 1 || limit > 100)
                throw new BusinessException(ErrorRecord.RequestInvalid);

            List<Guid>? storeIds = await GetStoreScopeAsync();
            var quantities = await GetSoldQuantitiesAsync(storeIds, DateTime.UtcNow.AddDays(-periodDays));
            List<MerchandiseBookReportItem> rows = await AddBookDetailsAsync(quantities);
            return Success(rows.OrderByDescending(item => item.QuantitySold)
                .ThenBy(item => item.Name).Take(limit).ToList());
        }

        public async Task<ApiResponse<List<MerchandiseBookReportItem>>> GetSlowMovingAsync(int periodDays, int maximumUnitsSold)
        {
            ValidateSalesParameters(periodDays);
            if (maximumUnitsSold < 0 || maximumUnitsSold > 1_000_000)
                throw new BusinessException(ErrorRecord.RequestInvalid);

            List<Guid>? storeIds = await GetStoreScopeAsync();
            var quantities = await GetSoldQuantitiesAsync(storeIds, DateTime.UtcNow.AddDays(-periodDays));
            List<MerchandiseBookReportItem> soldBooks = await AddBookDetailsAsync(quantities);
            Dictionary<Guid, int> soldByBook = soldBooks.ToDictionary(item => item.BookId, item => item.QuantitySold);

            var inventoryBooks = await GetInventoryQuery(storeIds)
                .GroupBy(item => new { item.BookId, item.Book!.Name, item.Book.Author })
                .Select(group => new MerchandiseBookReportItem
                {
                    BookId = group.Key.BookId,
                    Name = group.Key.Name,
                    Author = group.Key.Author,
                    AvailableQuantity = group.Sum(item => item.AvailableQuantity)
                })
                .ToListAsync();

            var rows = inventoryBooks
                .Select(item =>
                {
                    item.QuantitySold = soldByBook.GetValueOrDefault(item.BookId);
                    return item;
                })
                .Where(item => item.QuantitySold <= maximumUnitsSold)
                .OrderBy(item => item.QuantitySold)
                .ThenByDescending(item => item.AvailableQuantity)
                .ThenBy(item => item.Name)
                .ToList();

            return Success(rows);
        }

        private IQueryable<InventoryEntity> GetInventoryQuery(List<Guid>? storeIds)
        {
            var query = _context.Inventories.AsNoTracking()
                .Where(item => !item.IsDeleted && item.Store != null && !item.Store.IsDeleted &&
                    item.Book != null && !item.Book.IsDeleted &&
                    (item.Book.Status == BookStatus.Available || item.Book.Status == BookStatus.OutOfStock));
            return storeIds == null ? query : query.Where(item => storeIds.Contains(item.StoreId));
        }

        private async Task<Dictionary<Guid, int>> GetSoldQuantitiesAsync(List<Guid>? storeIds, DateTime startDate)
        {
            if (storeIds is { Count: 0 }) return new Dictionary<Guid, int>();

            var sales = await _context.OrderItems.AsNoTracking()
                .Where(item => item.Order != null && !item.Order.IsDeleted &&
                    item.Order.OrderStatus == OrderStatus.Completed && item.Order.CompletedAt >= startDate &&
                    item.Book != null && !item.Book.IsDeleted)
                .Select(item => new { item.OrderId, item.BookId, item.Quantity, item.Order!.OrderCode })
                .ToListAsync();

            if (sales.Count == 0) return new Dictionary<Guid, int>();

            var orderCodes = sales.Select(item => item.OrderCode).Distinct().ToList();
            var reservations = await _context.InventoryTransactions.AsNoTracking()
                .Where(transaction => orderCodes.Contains(transaction.Reason.Replace("Reserved for order ", "")) &&
                    transaction.Reason.StartsWith("Reserved for order ") &&
                    transaction.TransactionType == TransactionType.Adjustment && transaction.Quantity > 0 &&
                    !transaction.IsDeleted && transaction.Inventory != null && !transaction.Inventory.IsDeleted &&
                    transaction.Inventory.Store != null && !transaction.Inventory.Store.IsDeleted &&
                    (storeIds == null || storeIds.Contains(transaction.Inventory.StoreId)))
                .Select(transaction => new
                {
                    OrderCode = transaction.Reason.Substring("Reserved for order ".Length),
                    transaction.Inventory!.BookId,
                    transaction.Inventory.StoreId,
                    transaction.Quantity
                })
                .ToListAsync();

            var totals = new Dictionary<Guid, int>();
            foreach (var sale in sales)
            {
                var matching = reservations.Where(reservation =>
                    reservation.OrderCode == sale.OrderCode && reservation.BookId == sale.BookId).ToList();
                if (matching.Count == 0) continue;

                int totalReserved = matching.Sum(reservation => reservation.Quantity);
                int allocatedQuantity = storeIds == null
                    ? sale.Quantity
                    : Math.Min(sale.Quantity, totalReserved);
                totals[sale.BookId] = totals.GetValueOrDefault(sale.BookId) + allocatedQuantity;
            }

            return totals;
        }

        private async Task<List<MerchandiseBookReportItem>> AddBookDetailsAsync(Dictionary<Guid, int> quantities)
        {
            if (quantities.Count == 0) return new List<MerchandiseBookReportItem>();
            List<Guid> bookIds = quantities.Keys.ToList();
            var books = await _context.Books.AsNoTracking()
                .Where(book => bookIds.Contains(book.Id) && !book.IsDeleted)
                .Select(book => new { book.Id, book.Name, book.Author })
                .ToListAsync();
            return books.Select(book => new MerchandiseBookReportItem
                {
                    BookId = book.Id,
                    Name = book.Name,
                    Author = book.Author,
                    QuantitySold = quantities[book.Id]
                })
                .ToList();
        }

        private async Task<List<Guid>?> GetStoreScopeAsync()
        {
            Guid? userId = _currentUser.GetCurrentUserId();
            RoleUser? role = _currentUser.GetRoleCurrentUser();
            if (!userId.HasValue || userId.Value == Guid.Empty)
                throw new BusinessException(ErrorRecord.Forbidden);
            if (role == RoleUser.Admin) return null;
            if (role != RoleUser.Manager)
                throw new BusinessException(ErrorRecord.Forbidden);
            return await _storeStaffRepository.GetStoreIdsByUserIdAsync(userId.Value);
        }

        private static void ValidateSalesParameters(int periodDays)
        {
            if (periodDays < 1 || periodDays > 3650)
                throw new BusinessException(ErrorRecord.RequestInvalid);
        }

        private static ApiResponse<List<MerchandiseBookReportItem>> Success(List<MerchandiseBookReportItem> rows) =>
            ApiResponse<List<MerchandiseBookReportItem>>.Response(MessageRecord.Success, data: rows);
    }
}
