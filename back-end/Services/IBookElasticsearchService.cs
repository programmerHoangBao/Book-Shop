using back_end.Documents;
using back_end.DTOs.Books.Requests;
using back_end.Entities;

namespace back_end.Services
{
    public interface IBookElasticsearchService
    {
        Task CreateIndexBookAsync(BookEntity book);

        Task CreateIndexListBooksAsync(List<BookEntity> books);

        Task DeleteBookAsync(Guid bookId);

        Task<bool> ExistsBookAsync(Guid bookId);

        Task<List<BookDocument>> SearchBooksAsync(SearchBooksRequest req);
    }
}
