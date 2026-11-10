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

        public async Task<bool> AddBookAsync(
            BookEntity book,
            Guid categoryId,
            List<string> images,
            List<CreateBookInventoryRequest> inventories
        )
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Books.AddAsync(book);
                await _context.BookCategories.AddAsync(new BookCategoryEntity
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                });

                if (images.Count > 0)
                {
                    await _context.BookImages.AddRangeAsync(images.Select(image => new BookImageEntity
                    {
                        BookId = book.Id,
                        ImageUrl = image
                    }));
                }

                if (inventories.Count > 0)
                {
                    await _context.Inventories.AddRangeAsync(inventories.Select(inventory => new InventoryEntity
                    {
                        BookId = book.Id,
                        StoreId = inventory.StoreId,
                        Quantity = inventory.Quantity,
                        AvailableQuantity = inventory.Quantity
                    }));
                }

                var result = await _context.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<BookEntity?> GetBookForUpdateAsync(Guid bookId)
        {
            return _context.Books
                .Include(book => book.Images)
                .Include(book => book.BookCategories)
                .FirstOrDefaultAsync(book => book.Id == bookId && !book.IsDeleted);
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
    }
}
