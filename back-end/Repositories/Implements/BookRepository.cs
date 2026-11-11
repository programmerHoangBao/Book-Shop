using back_end.Data;
using back_end.DTOs.Books.Requests;
using back_end.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class BookRepository : IBookRepository
    {
        private readonly DatabaseContext _context;

        public BookRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task<BookEntity?> FindActiveBookAsync(string name, string? author, string? publisher)
        {
            string normalizedName = name.Trim().ToLower();
            string? normalizedAuthor = author?.Trim().ToLower();
            string? normalizedPublisher = publisher?.Trim().ToLower();

            return _context.Books
                .AsNoTracking()
                .Include(book => book.Images)
                .Include(book => book.BookCategories.Where(category => !category.IsDeleted))
                .Where(book => !book.IsDeleted
                    && book.Name.Trim().ToLower() == normalizedName
                    && (book.Author == null
                        ? normalizedAuthor == null
                        : book.Author.Trim().ToLower() == normalizedAuthor)
                    && (book.Publisher == null
                        ? normalizedPublisher == null
                        : book.Publisher.Trim().ToLower() == normalizedPublisher))
                .FirstOrDefaultAsync();
        }

        public Task<BookEntity?> GetBookForUpdateAsync(Guid bookId)
        {
            return _context.Books
                .Include(book => book.Images)
                .Include(book => book.BookCategories)
                .FirstOrDefaultAsync(book => book.Id == bookId && !book.IsDeleted);
        }

        public Task<BookEntity?> GetDeletedBookForUpdateAsync(Guid bookId)
        {
            return _context.Books
                .Include(book => book.Images)
                .Include(book => book.BookCategories)
                .FirstOrDefaultAsync(book => book.Id == bookId && book.IsDeleted);
        }

        public Task<BookEntity?> GetBookDetailsAsync(Guid bookId)
        {
            return _context.Books
                .AsNoTracking()
                .Where(book => book.Id == bookId && !book.IsDeleted)
                .Include(book => book.Images)
                .Include(book => book.BookCategories.Where(bookCategory => !bookCategory.IsDeleted))
                    .ThenInclude(bookCategory => bookCategory.Category)
                .Include(book => book.Inventories.Where(inventory => !inventory.IsDeleted && inventory.Store != null && !inventory.Store.IsDeleted))
                    .ThenInclude(inventory => inventory.Store)
                .Include(book => book.Reviews.Where(review => !review.IsDeleted))
                .FirstOrDefaultAsync();
        }

        public async Task<(List<BookEntity> Books, int TotalItems)> GetDeletedBooksAsync(
            int pageNumber, int pageSize)
        {
            IQueryable<BookEntity> query = _context.Books
                .AsNoTracking()
                .Where(book => book.IsDeleted);

            int totalItems = await query.CountAsync();
            List<BookEntity> books = await query
                .OrderByDescending(book => book.UpdatedAt)
                .ThenByDescending(book => book.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (books, totalItems);
        }

        public async Task<bool> UpdateBookAsync(
            BookEntity book,
            Guid categoryId,
            List<string> images)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (book.BookCategories.Count != 1 || book.BookCategories.First().CategoryId != categoryId)
                {
                    _context.BookCategories.RemoveRange(book.BookCategories);
                    book.BookCategories.Clear();
                    book.BookCategories.Add(new BookCategoryEntity
                    {
                        BookId = book.Id,
                        CategoryId = categoryId
                    });
                }

                _context.BookImages.RemoveRange(book.Images);
                book.Images.Clear();
                foreach (string image in images)
                {
                    book.Images.Add(new BookImageEntity
                    {
                        BookId = book.Id,
                        ImageUrl = image
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteBookAsync(BookEntity book)
        {
            book.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreBookAsync(BookEntity book)
        {
            book.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
