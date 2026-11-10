using back_end.DTOs;
using back_end.DTOs.Carts.Requests;

namespace back_end.Services
{
    public interface ICartService
    {
        Task<ApiResponse<object?>> GetBooksAsync();
        Task<ApiResponse<object?>> AddBookAsync(AddBookToCartRequest request);
        Task<ApiResponse<object?>> RemoveBookAsync(Guid bookId);
    }
}
