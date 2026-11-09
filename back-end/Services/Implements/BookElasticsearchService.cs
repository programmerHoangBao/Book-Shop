using AutoMapper;
using back_end.Documents;
using back_end.DTOs.Books.Requests;
using back_end.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace back_end.Services.Implements
{
    public class BookElasticsearchService : IBookElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly IMapper _mapper;
        private readonly ILogger<BookElasticsearchService> _logger;

        public BookElasticsearchService(
            ElasticsearchClient client,
            IMapper mapper,
            ILogger<BookElasticsearchService> logger
        )
        {
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task IndexBookAsync(BookEntity book)
        {
            BookDocument document = _mapper.Map<BookDocument>(book);
            var response = await _client.IndexAsync(
                document,
                idx => idx.Id(document.Id)
            );
            if (!response.IsValidResponse)
            {
                throw new Exception(
                    $"Failed to index book: {response.DebugInformation}"
                );
            }
        }

        public async Task IndexListBooksAsync(List<BookEntity> books)
        {
            List<BookDocument> documents = books
                    .Select(b => _mapper.Map<BookDocument>(b))
                    .ToList();
            var response = await _client.BulkAsync(b => b
                .IndexMany(documents, (op, document) =>
                    op.Id(document.Id)
                )
            );
            if (!response.IsValidResponse || response.Errors)
            {
                throw new Exception(
                    $"Failed to bulk index books: {response.DebugInformation}"
                );
            }
        }
        public async Task DeleteBookAsync(Guid bookId)
        {
            var response = await _client.DeleteAsync<BookDocument>(bookId);

            if (!response.IsValidResponse)
            {
                throw new Exception(
                    $"Failed to delete book: {response.DebugInformation}"
                );
            }
        }

        public async Task<bool> ExistsBookAsync(Guid bookId)
        {
            var response = await _client.ExistsAsync<BookDocument>(bookId);

            return response.Exists;
        }

        public async Task<List<BookDocument>> SearchBooksAsync(SearchBooksRequest req)
        {
            var response = await _client.SearchAsync<BookDocument>(s => s
                    .From((req.PageNumber - 1) * req.PageSize)
                    .Size(req.PageSize)
                    .Query(q => q
                        .Bool(b => b
                        .Must(m => m
                                .MultiMatch(mm => mm
                                    .Query(req.Key ?? string.Empty)
                                    .Fields(new[]
                                    {
                            "name",
                            "author",
                            "publisher",
                            "description"
                                    })
                                    .Type(TextQueryType.BestFields)
                                    .Fuzziness("AUTO")
                                )
                        )
                        .Filter(f =>
                            {
                                if (req.Book.HasValue)
                                {
                                    f.Term(t => t
                                        .Field(x => x.Status)
                                        .Value(req.Book.Value.ToString())
                                    );
                                }

                                if (req.IsDeleted.HasValue)
                                {
                                    f.Term(t => t
                                        .Field(x => x.IsDeleted)
                                        .Value(req.IsDeleted.Value)
                                    );
                                }
                            })
                        )
                    )
                );
            if (!response.IsValidResponse)
            {
                _logger.LogError($"Search failed key = {req.Key} logs: {response.DebugInformation}");
                return new List<BookDocument>();
            }
            var books = response.Documents.ToList();
            return books;
        }
    }
}
