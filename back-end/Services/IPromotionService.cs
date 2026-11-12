using back_end.DTOs;
using back_end.DTOs.Promotions.Requests;
using back_end.DTOs.Promotions.Responses;

namespace back_end.Services
{
    public interface IPromotionService
    {
        Task<ApiResponse<PromotionResponse>> CreateAsync(CreatePromotionRequest request);
        Task<ApiResponse<PaginationResponse<PromotionSuggestionResponse>>> GetSuggestionsForBookAsync(
            Guid bookId,
            int pageNumber,
            int pageSize);
    }
}
