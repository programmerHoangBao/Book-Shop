using back_end.Data;
using back_end.Entities;

namespace back_end.Repositories.Implements
{
    public class BookRepository : IBookRepository
    {
        private readonly DatabaseContext _context;

        public BookRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> AddBookAsync(BookEntity book, List<string>? images = null)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Books.AddAsync(book);
                var result = await _context.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                if (images != null && images.Count > 0)
                {
                    await _context.BookImages.AddRangeAsync(images.Select(image => new BookImageEntity
                    {
                        BookId = book.Id,
                        ImageUrl = image
                    }));
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
