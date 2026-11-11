using back_end.Data;
using back_end.DTOs.Books.Requests;
using back_end.Entities;
using back_end.Enums;
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

        public async Task<(BookEntity Book, InventoryEntity Inventory)> CreateBookAndAddToInventoryAsync(
            BookEntity book,
            Guid storeId,
            Guid userId,
            int quantity,
            string reason)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            try
            {
                book.Name = book.Name.Trim();
                book.Author = book.Author?.Trim();
                book.Publisher = book.Publisher?.Trim();
                _context.Books.Add(book);
                InventoryEntity inventory = await AddInventoryQuantityAsync(
                    book, storeId, userId, quantity, reason);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (book, inventory);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(BookEntity Book, InventoryEntity Inventory)?> AddExistingBookToInventoryAsync(
            Guid bookId,
            Guid storeId,
            Guid userId,
            int quantity,
            string reason)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            try
            {
                BookEntity? book = await _context.Books
                    .FirstOrDefaultAsync(candidate => candidate.Id == bookId && !candidate.IsDeleted);
                if (book == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                InventoryEntity inventory = await AddInventoryQuantityAsync(
                    book, storeId, userId, quantity, reason);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (book, inventory);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<InventoryEntity> AddInventoryQuantityAsync(
            BookEntity book,
            Guid storeId,
            Guid userId,
            int quantity,
            string reason)
        {
            InventoryEntity? inventory = await _context.Inventories
                .FirstOrDefaultAsync(item => item.StoreId == storeId
                    && item.BookId == book.Id
                    && !item.IsDeleted);

            int quantityBefore = inventory?.Quantity ?? 0;
            if (inventory == null)
            {
                inventory = new InventoryEntity
                {
                    StoreId = storeId,
                    BookId = book.Id
                };
                _context.Inventories.Add(inventory);
            }

            checked
            {
                inventory.Quantity += quantity;
                inventory.AvailableQuantity += quantity;
            }

            _context.InventoryTransactions.Add(new InventoryTransactionEntity
            {
                Inventory = inventory,
                UserId = userId,
                TransactionType = TransactionType.Import,
                Quantity = quantity,
                QuntityBefore = quantityBefore,
                QuantityAfter = inventory.Quantity,
                Reason = reason
            });

            return inventory;
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

        public async Task<(List<InventoryEntity> Inventories, int TotalItems)> GetStoreInventoryBooksAsync(
            Guid storeId,
            int pageNumber,
            int pageSize)
        {
            IQueryable<InventoryEntity> query = _context.Inventories
                .AsNoTracking()
                .Where(inventory => inventory.StoreId == storeId &&
                    !inventory.IsDeleted &&
                    inventory.Book != null &&
                    !inventory.Book.IsDeleted);

            int totalItems = await query.CountAsync();
            List<InventoryEntity> inventories = await query
                .Include(inventory => inventory.Book!)
                    .ThenInclude(book => book.Images)
                .OrderBy(inventory => inventory.Book!.Name)
                .ThenBy(inventory => inventory.BookId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (inventories, totalItems);
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
