using back_end.Entities;

namespace back_end.Repositories
{
    public interface IBookRepository
    {
        Task<bool> AddBookAsync(BookEntity book, List<string>? images = default);
    }
}
