using back_end.DTOs;
using back_end.DTOs.CustomerReports;

namespace back_end.Services
{
    public interface ICustomerReportService
    {
        Task<ApiResponse<CustomerCountReportResponse>> GetNewCustomersAsync(DateTime? startDate, DateTime? endDate);
        Task<ApiResponse<CustomerCountReportResponse>> GetReturningCustomersAsync(DateTime? startDate, DateTime? endDate);
        Task<ApiResponse<CustomerPurchaseFrequencyResponse>> GetPurchaseFrequencyAsync(DateTime? startDate, DateTime? endDate);
    }
}
