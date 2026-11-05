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
    }
}
