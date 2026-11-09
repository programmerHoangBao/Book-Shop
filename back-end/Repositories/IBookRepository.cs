using back_end.DTOs.Books.Requests;
using back_end.Entities;

namespace back_end.Repositories
{
    public interface IBookRepository
    {
        Task<bool> AddBookAsync(
            BookEntity book,
            Guid categoryId,
            List<string> images,
            List<CreateBookInventoryRequest> inventories
        );

        Task<BookEntity?> GetBookForUpdateAsync(Guid bookId);

        Task<BookEntity?> GetBookDetailsAsync(Guid bookId);

        Task<bool> UpdateBookAsync(
            BookEntity book,
            Guid categoryId,
            List<string> images
        );
    }
}
