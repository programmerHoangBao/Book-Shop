using back_end.DTOs;
using back_end.DTOs.Books.Requests;

namespace back_end.Services
{
    public interface IBookService
    {
        Task<ApiResponse<object?>> CreateBookAsync(CreateBookRequest request);
        Task<ApiResponse<object?>> UpdateBookAsync(Guid bookId, UpdateBookRequest request);
        Task<ApiResponse<object?>> SoftDeleteBookAsync(Guid bookId);
        Task<ApiResponse<object?>> RestoreBookAsync(Guid bookId);
        Task<ApiResponse<object?>> GetBookDetailsAsync(Guid bookId);
    }
}
