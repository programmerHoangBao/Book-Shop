using back_end.DTOs;

namespace back_end.Services
{
    public interface IRevenueReportService
    {
        Task<RevenueReportResponse> GetByPeriodAsync(string period, DateTime? startDate, DateTime? endDate, Guid? storeId);
        Task<RevenueReportResponse> GetByStoreAsync(DateTime? startDate, DateTime? endDate, Guid? storeId);
        Task<RevenueReportResponse> GetByProductAsync(DateTime? startDate, DateTime? endDate, Guid? storeId);
        Task<RevenueReportResponse> GetByCategoryAsync(DateTime? startDate, DateTime? endDate, Guid? storeId);
    }
}
