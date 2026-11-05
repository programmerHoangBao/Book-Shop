using back_end.DTOs;
using back_end.DTOs.Books.Requests;

namespace back_end.Services
{
    public interface IBookService
    {
        Task<ApiResponse<object?>> CreateBookAsync(CreateBookRequest request);
    }
}
