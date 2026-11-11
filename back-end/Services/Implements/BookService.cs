using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Books.Requests;
using back_end.DTOs.Books.Responses;
using back_end.Documents;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Repositories;
using System.Text.Json;

namespace back_end.Services.Implements
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreStaffRepository _storeStaffRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IBookElasticsearchService _bookElasticsearchService;
        private readonly IMapper _mapper;

        public BookService(
            IBookRepository bookRepository, 
            ICategoryRepository categoryRepository, 
            IStoreRepository storeRepository,
            IStoreStaffRepository storeStaffRepository,
            ICurrentUserService currentUserService,
            IKafkaProducer kafkaProducer,
            IBookElasticsearchService bookElasticsearchService,
            IMapper mapper
        )
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _storeStaffRepository = storeStaffRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
            _bookElasticsearchService = bookElasticsearchService;
        }

        public async Task<ApiResponse<object?>> UpdateBookAsync(Guid bookId, UpdateBookRequest request)
        {
            if (bookId == Guid.Empty || request.CategoryId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            BookEntity? book = await _bookRepository.GetBookForUpdateAsync(bookId);
            if (book == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            if (!await _categoryRepository.CategoryExistsAsync(request.CategoryId))
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            List<string> imageUrls = (request.ImageUrls ?? new List<string>())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            book.Name = request.Name.Trim();
            book.Author = request.Author?.Trim();
            book.Publisher = request.Publisher?.Trim();
            book.Description = request.Description;
            book.Price = request.Price;

            if (!await _bookRepository.UpdateBookAsync(book, request.CategoryId, imageUrls))
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            BookDocumentMessage bookDocumentMessage = _mapper.Map<BookDocumentMessage>(book);
            await _kafkaProducer.ProduceAsync(
                "index-book-elasticsearch",
                bookDocumentMessage
            );

            UpdateBookResponse response = new()
            {
                Id = book.Id,
                Name = book.Name,
                Author = book.Author,
                Publisher = book.Publisher,
                Description = book.Description,
                Price = book.Price,
                CategoryId = request.CategoryId,
                ImageUrls = imageUrls
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        public async Task<ApiResponse<object?>> AddNewBookToInventoryAsync(
            AddBookToInventoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Quantity <= 0 ||
                request.CategoryId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if (!await _categoryRepository.CategoryExistsAsync(request.CategoryId))
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            var (userId, storeId) = await ResolveInventoryTargetAsync(request.StoreId);

            List<string> imageUrls = (request.ImageUrls ?? new List<string>())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            BookEntity book = new()
            {
                Name = request.Name.Trim(),
                Author = request.Author?.Trim(),
                Publisher = request.Publisher?.Trim(),
                Description = request.Description,
                Price = request.Price
            };
            book.BookCategories.Add(new BookCategoryEntity
            {
                BookId = book.Id,
                CategoryId = request.CategoryId
            });

            foreach (string imageUrl in imageUrls)
            {
                book.Images.Add(new BookImageEntity
                {
                    BookId = book.Id,
                    ImageUrl = imageUrl
                });
            }

            var (savedBook, inventory) = await _bookRepository.CreateBookAndAddToInventoryAsync(
                book,
                storeId,
                userId,
                request.Quantity,
                "Stock imported");

            BookDocumentMessage bookDocumentMessage = _mapper.Map<BookDocumentMessage>(savedBook);
            await _kafkaProducer.ProduceAsync("index-book-elasticsearch", bookDocumentMessage);

            AddBookToInventoryResponse response = new()
            {
                BookId = savedBook.Id,
                InventoryId = inventory.Id,
                QuantityAdded = request.Quantity,
                Quantity = inventory.Quantity,
                AvailableQuantity = inventory.AvailableQuantity,
                BookCreated = true
            };

            return ApiResponse<object?>.Response(
                MessageRecord.CreatedSuccessfully,
                data: response);
        }

        public async Task<ApiResponse<object?>> AddExistingBookToInventoryAsync(
            Guid bookId,
            AddExistingBookToInventoryRequest request)
        {
            if (bookId == Guid.Empty || request.Quantity <= 0)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var (userId, storeId) = await ResolveInventoryTargetAsync(request.StoreId);
            var result = await _bookRepository.AddExistingBookToInventoryAsync(
                bookId,
                storeId,
                userId,
                request.Quantity,
                "Stock imported");

            if (result == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            var (book, inventory) = result.Value;
            AddBookToInventoryResponse response = new()
            {
                BookId = book.Id,
                InventoryId = inventory.Id,
                QuantityAdded = request.Quantity,
                Quantity = inventory.Quantity,
                AvailableQuantity = inventory.AvailableQuantity,
                BookCreated = false
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        private async Task<(Guid UserId, Guid StoreId)> ResolveInventoryTargetAsync(
            Guid? requestedStoreId)
        {
            Guid? currentUserId = _currentUserService.GetCurrentUserId();
            if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            Guid storeId;
            RoleUser? role = _currentUserService.GetRoleCurrentUser();
            if (role == RoleUser.Manager)
            {
                StoreEntity? assignedStore = await _storeStaffRepository.GetStoreByUserIdAsync(currentUserId.Value);
                if (assignedStore == null || assignedStore.IsDeleted ||
                    (requestedStoreId.HasValue && requestedStoreId.Value != assignedStore.Id))
                {
                    throw new BusinessException(ErrorRecord.Forbidden);
                }
                storeId = assignedStore.Id;
            }
            else if (role == RoleUser.Admin && requestedStoreId.HasValue && requestedStoreId.Value != Guid.Empty)
            {
                storeId = requestedStoreId.Value;
            }
            else
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            StoreEntity? store = await _storeRepository.GetStoreByIdAsync(storeId);
            if (store == null || store.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.StoreNotFound);
            }

            return (currentUserId.Value, storeId);
        }

        public async Task<ApiResponse<object?>> SoftDeleteBookAsync(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            BookEntity? book = await _bookRepository.GetBookForUpdateAsync(bookId);
            if (book == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            if (!await _bookRepository.SoftDeleteBookAsync(book))
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            BookDocumentMessage bookDocumentMessage = _mapper.Map<BookDocumentMessage>(book);
            await _kafkaProducer.ProduceAsync(
                "index-book-elasticsearch",
                bookDocumentMessage
            );

            return ApiResponse<object?>.Response(MessageRecord.Success);
        }

        public async Task<ApiResponse<object?>> RestoreBookAsync(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            BookEntity? book = await _bookRepository.GetDeletedBookForUpdateAsync(bookId);
            if (book == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            if (!await _bookRepository.RestoreBookAsync(book))
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            BookDocumentMessage bookDocumentMessage = _mapper.Map<BookDocumentMessage>(book);
            await _kafkaProducer.ProduceAsync(
                "index-book-elasticsearch",
                bookDocumentMessage
            );

            return ApiResponse<object?>.Response(MessageRecord.Success);
        }

        public async Task<ApiResponse<List<BookDocument>>> SearchBooksAsync(
            SearchBooksRequest request)
        {
            if (request.PageNumber < 1 ||
                request.PageSize < 1 ||
                request.PageSize > 100 ||
                (long)(request.PageNumber - 1) * request.PageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            request.IsDeleted = false;
            List<BookDocument> books = await _bookElasticsearchService.SearchBooksAsync(request);

            return ApiResponse<List<BookDocument>>.Response(
                MessageRecord.Success, data: books);
        }

        public async Task<ApiResponse<CursorPaginationResponse<BookDocument>>> GetBooksAsync(
            string? cursor, int pageSize)
        {
            if (pageSize < 1 || pageSize > 100)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            BookRankCursor? after = DecodeBookCursor(cursor);
            List<BookDocument> books = await _bookElasticsearchService.SearchBooksAfterAsync(
                after?.Rank,
                after?.BookId,
                pageSize + 1);

            bool hasNextPage = books.Count > pageSize;
            if (hasNextPage)
            {
                books.RemoveAt(books.Count - 1);
            }

            string? nextCursor = hasNextPage && books.Count > 0
                ? EncodeBookCursor(books[^1])
                : null;

            CursorPaginationResponse<BookDocument> response = new()
            {
                Items = books,
                NextCursor = nextCursor
            };

            return ApiResponse<CursorPaginationResponse<BookDocument>>.Response(
                MessageRecord.Success,
                data: response);
        }

        public async Task<ApiResponse<PaginationResponse<BookDocument>>> GetDeletedBooksAsync(
            int pageNumber, int pageSize)
        {
            if (pageNumber < 1 ||
                pageSize < 1 ||
                pageSize > 100 ||
                (long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var result = await _bookRepository.GetDeletedBooksAsync(pageNumber, pageSize);
            PaginationResponse<BookDocument> response = new()
            {
                Items = _mapper.Map<List<BookDocument>>(result.Books),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize)
            };

            return ApiResponse<PaginationResponse<BookDocument>>.Response(
                MessageRecord.Success,
                data: response);
        }

        public async Task<ApiResponse<PaginationResponse<BookDocument>>> SearchDeletedBooksAsync(
            SearchBooksRequest request)
        {
            if (request.PageNumber < 1 ||
                request.PageSize < 1 ||
                request.PageSize > 100 ||
                (long)(request.PageNumber - 1) * request.PageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            request.IsDeleted = true;
            var result = await _bookElasticsearchService.SearchDeletedBooksAsync(request);
            PaginationResponse<BookDocument> response = new()
            {
                Items = result.Books,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / request.PageSize)
            };

            return ApiResponse<PaginationResponse<BookDocument>>.Response(
                MessageRecord.Success,
                data: response);
        }

        private static string EncodeBookCursor(BookDocument book)
        {
            byte[] cursorBytes = JsonSerializer.SerializeToUtf8Bytes(
                new BookRankCursor { Rank = book.Rank, BookId = book.Id });

            return Convert.ToBase64String(cursorBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static BookRankCursor? DecodeBookCursor(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor))
            {
                return null;
            }

            if (cursor.Length > 512)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            try
            {
                string base64 = cursor.Replace('-', '+').Replace('_', '/');
                base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

                BookRankCursor? decoded = JsonSerializer.Deserialize<BookRankCursor>(
                    Convert.FromBase64String(base64));

                if (decoded == null || decoded.BookId == Guid.Empty)
                {
                    throw new FormatException("Cursor payload is invalid.");
                }

                return decoded;
            }
            catch (Exception exception) when (
                exception is FormatException or JsonException or ArgumentException)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }
        }

        private sealed class BookRankCursor
        {
            public BookRankCursor() { }

            public long Rank { get; set; }
            public Guid BookId { get; set; }
        }

        public async Task<ApiResponse<object?>> GetBookDetailsAsync(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            BookEntity? book = await _bookRepository.GetBookDetailsAsync(bookId);
            if (book == null)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            var response = new BookDetailResponse
            {
                Id = book.Id,
                Name = book.Name,
                Author = book.Author,
                Publisher = book.Publisher,
                Description = book.Description,
                Price = book.Price,
                Status = book.Status,
                Rank = book.Rank,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                Category = book.BookCategories
                    .Where(bookCategory => bookCategory.Category != null && !bookCategory.Category.IsDeleted)
                    .Select(bookCategory => new BookDetailCategoryResponse
                    {
                        Id = bookCategory.Category!.Id,
                        Name = bookCategory.Category.Name
                    })
                    .FirstOrDefault(),
                ImageUrls = book.Images.Select(image => image.ImageUrl).ToList(),
                Inventories = book.Inventories.Select(inventory => new BookDetailInventoryResponse
                {
                    StoreId = inventory.StoreId,
                    StoreCode = inventory.Store!.StoreCode,
                    StoreAddress = inventory.Store.Address,
                    AvailableQuantity = inventory.AvailableQuantity
                }).ToList(),
                AverageRating = book.Reviews.Count == 0 ? null : book.Reviews.Average(review => review.Ratting),
                ReviewCount = book.Reviews.Count
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }
    }
}
