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
