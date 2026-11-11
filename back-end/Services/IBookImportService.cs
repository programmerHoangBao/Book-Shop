using back_end.DTOs;
using back_end.DTOs.Books.Requests;

namespace back_end.Services
{
    public interface IBookImportService
    {
        Task<ApiResponse<object?>> ImportNewBooksAsync(ImportBooksFileRequest request);
        Task<ApiResponse<object?>> ImportExistingBooksAsync(ImportBooksFileRequest request);
    }
}
