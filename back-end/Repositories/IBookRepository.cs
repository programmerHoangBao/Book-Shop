using back_end.DTOs.Books.Requests;
using back_end.Entities;

namespace back_end.Repositories
{
    public interface IBookRepository
    {
        Task<BookEntity?> FindActiveBookAsync(string name, string? author, string? publisher);

        Task<(BookEntity Book, InventoryEntity Inventory)> CreateBookAndAddToInventoryAsync(
            BookEntity book,
            Guid storeId,
            Guid userId,
            int quantity,
            string reason);
        Task<(BookEntity Book, InventoryEntity Inventory)?> AddExistingBookToInventoryAsync(
            Guid bookId,
            Guid storeId,
            Guid userId,
            int quantity,
            string reason);

        Task<BookEntity?> GetBookForUpdateAsync(Guid bookId);
        Task<BookEntity?> GetDeletedBookForUpdateAsync(Guid bookId);

        Task<BookEntity?> GetBookDetailsAsync(Guid bookId);

        Task<(List<BookEntity> Books, int TotalItems)> GetDeletedBooksAsync(
            int pageNumber, int pageSize);

        Task<bool> UpdateBookAsync(
            BookEntity book,
            Guid categoryId,
            List<string> images
        );

        Task<bool> SoftDeleteBookAsync(BookEntity book);
        Task<bool> RestoreBookAsync(BookEntity book);
    }
}
