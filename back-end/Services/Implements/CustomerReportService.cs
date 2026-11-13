using back_end.Data;
using back_end.DTOs;
using back_end.DTOs.CustomerReports;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using Microsoft.EntityFrameworkCore;

namespace back_end.Services.Implements
{
    public class CustomerReportService : ICustomerReportService
    {
        private readonly DatabaseContext _context;

        public CustomerReportService(DatabaseContext context) => _context = context;

        public async Task<ApiResponse<CustomerCountReportResponse>> GetNewCustomersAsync(
            DateTime? startDate,
            DateTime? endDate)
        {
            var (start, end) = ResolveDateRange(startDate, endDate);
            int count = await _context.Users.AsNoTracking()
                .Where(user => user.Role == RoleUser.Customer && !user.IsDeleted &&
                    user.CreatedAt >= start && user.CreatedAt < end)
                .CountAsync();

            return ApiResponse<CustomerCountReportResponse>.Response(
                MessageRecord.Success,
                data: new CustomerCountReportResponse
                {
                    StartDate = start,
                    EndDate = end,
                    CustomerCount = count
                });
        }

        public async Task<ApiResponse<CustomerCountReportResponse>> GetReturningCustomersAsync(
            DateTime? startDate,
            DateTime? endDate)
        {
            var (start, end) = ResolveDateRange(startDate, endDate);
            int count = await _context.Users.AsNoTracking()
                .Where(user => user.Role == RoleUser.Customer && !user.IsDeleted &&
                    _context.Orders.Any(order => order.UserId == user.Id && !order.IsDeleted &&
                        order.OrderStatus == OrderStatus.Completed &&
                        order.CompletedAt >= start && order.CompletedAt < end) &&
                    _context.Orders.Any(previousOrder => previousOrder.UserId == user.Id &&
                        !previousOrder.IsDeleted && previousOrder.OrderStatus == OrderStatus.Completed &&
                        previousOrder.CompletedAt < start))
                .CountAsync();

            return ApiResponse<CustomerCountReportResponse>.Response(
                MessageRecord.Success,
                data: new CustomerCountReportResponse
                {
                    StartDate = start,
                    EndDate = end,
                    CustomerCount = count
                });
        }

        public async Task<ApiResponse<CustomerPurchaseFrequencyResponse>> GetPurchaseFrequencyAsync(
            DateTime? startDate,
            DateTime? endDate)
        {
            var (start, end) = ResolveDateRange(startDate, endDate);
            var purchasesByCustomer = await _context.Orders.AsNoTracking()
                .Where(order => !order.IsDeleted && order.OrderStatus == OrderStatus.Completed &&
                    order.CompletedAt >= start && order.CompletedAt < end &&
                    order.User != null && order.User.Role == RoleUser.Customer && !order.User.IsDeleted)
                .GroupBy(order => order.UserId)
                .Select(group => group.Count())
                .ToListAsync();

            var distribution = purchasesByCustomer
                .GroupBy(orderCount => orderCount)
                .OrderBy(group => group.Key)
                .Select(group => new CustomerPurchaseFrequencyItem
                {
                    CompletedOrderCount = group.Key,
                    CustomerCount = group.Count()
                })
                .ToList();

            int totalOrders = purchasesByCustomer.Sum();
            int customerCount = purchasesByCustomer.Count;
            var report = new CustomerPurchaseFrequencyResponse
            {
                StartDate = start,
                EndDate = end,
                TotalCompletedOrders = totalOrders,
                PurchasingCustomerCount = customerCount,
                AveragePurchasesPerCustomer = customerCount == 0
                    ? 0
                    : Math.Round((decimal)totalOrders / customerCount, 2),
                FrequencyDistribution = distribution
            };

            return ApiResponse<CustomerPurchaseFrequencyResponse>.Response(
                MessageRecord.Success,
                data: report);
        }

        private static (DateTime Start, DateTime End) ResolveDateRange(DateTime? startDate, DateTime? endDate)
        {
            DateTime now = DateTime.UtcNow;
            DateTime end = NormalizeUtc(endDate ?? now);
            DateTime start = NormalizeUtc(startDate ?? end.AddDays(-30));
            if (start >= end)
                throw new BusinessException(ErrorRecord.RequestInvalid);
            return (start, end);
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
