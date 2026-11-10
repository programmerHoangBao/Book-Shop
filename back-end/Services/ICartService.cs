using back_end.DTOs;
using back_end.DTOs.Carts.Responses;
using back_end.DTOs.Carts.Requests;

namespace back_end.Services
{
    public interface ICartService
    {
        Task<ApiResponse<PaginationResponse<CartItemResponse>>> GetBooksAsync(
            int pageNumber, int pageSize);
        Task<ApiResponse<object?>> AddBookAsync(AddBookToCartRequest request);
        Task<ApiResponse<object?>> UpdateQuantityAsync(
            Guid bookId, UpdateCartItemQuantityRequest request);
        Task<ApiResponse<object?>> RemoveBookAsync(Guid bookId);
    }
}
