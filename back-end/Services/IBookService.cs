using back_end.DTOs;
using back_end.DTOs.Books.Requests;
using back_end.Documents;

namespace back_end.Services
{
    public interface IBookService
    {
        Task<ApiResponse<object?>> CreateBookAsync(CreateBookRequest request);
        Task<ApiResponse<object?>> UpdateBookAsync(Guid bookId, UpdateBookRequest request);
        Task<ApiResponse<object?>> SoftDeleteBookAsync(Guid bookId);
        Task<ApiResponse<object?>> RestoreBookAsync(Guid bookId);
        Task<ApiResponse<List<BookDocument>>> SearchBooksAsync(SearchBooksRequest request);
        Task<ApiResponse<CursorPaginationResponse<BookDocument>>> GetBooksAsync(
            string? cursor, int pageSize);
        Task<ApiResponse<PaginationResponse<BookDocument>>> GetDeletedBooksAsync(
            int pageNumber, int pageSize);
        Task<ApiResponse<PaginationResponse<BookDocument>>> SearchDeletedBooksAsync(
            SearchBooksRequest request);
        Task<ApiResponse<object?>> GetBookDetailsAsync(Guid bookId);
    }
}
