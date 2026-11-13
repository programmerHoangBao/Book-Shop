using back_end.DTOs;
using back_end.DTOs.Merchandise;

namespace back_end.Services
{
    public interface IMerchandiseReportService
    {
        Task<ApiResponse<List<MerchandiseBookReportItem>>> GetLowStockAsync(int threshold);
        Task<ApiResponse<List<MerchandiseBookReportItem>>> GetOutOfStockAsync();
        Task<ApiResponse<List<MerchandiseBookReportItem>>> GetBestSellingAsync(int periodDays, int limit);
        Task<ApiResponse<List<MerchandiseBookReportItem>>> GetSlowMovingAsync(int periodDays, int maximumUnitsSold);
    }
}
