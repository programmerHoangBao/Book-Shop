using back_end.DTOs;
using back_end.DTOs.Reviews.Requests;

namespace back_end.Services
{
    public interface IReviewService
    {
        Task<ApiResponse<object?>> CreateReviewAsync(CreateReviewRequest request);
        Task<ApiResponse<object?>> GetBookReviewsAsync(Guid bookId, int pageNumber, int pageSize);
    }
}
