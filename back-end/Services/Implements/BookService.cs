using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Books.Requests;
using back_end.DTOs.Books.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IMapper _mapper;

        public BookService(
            IBookRepository bookRepository, 
            ICategoryRepository categoryRepository, 
            IStoreRepository storeRepository,
            IKafkaProducer kafkaProducer,
            IMapper mapper
        )
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ApiResponse<object?>> CreateBookAsync(CreateBookRequest request)
        {
            List<CreateBookInventoryRequest> inventories = request.Inventories ?? new();
            List<string> requestedImageUrls = request.ImageUrls ?? new();

            if (request.CategoryId == Guid.Empty ||
                !await _categoryRepository.CategoryExistsAsync(request.CategoryId))
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            List<Guid> requestedStoreIds = inventories
                .Select(inventory => inventory.StoreId)
                .ToList();

            if (requestedStoreIds.Any(id => id == Guid.Empty) ||
                requestedStoreIds.Distinct().Count() != requestedStoreIds.Count)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if (requestedStoreIds.Count > 0)
            {
                List<Guid> activeStoreIds = await _storeRepository.GetActiveStoreIdsAsync(requestedStoreIds);
                if (activeStoreIds.Count != requestedStoreIds.Count)
                {
                    throw new BusinessException(ErrorRecord.StoreNotFound);
                }
            }

            List<string> imageUrls = requestedImageUrls
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

            bool created = await _bookRepository.AddBookAsync(
                book,
                request.CategoryId,
                imageUrls,
                inventories
            );

            if (!created)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            BookDocumentMessage bookDocumentMessage = _mapper.Map<BookDocumentMessage>(book);
            await _kafkaProducer.ProduceAsync(
                "index-book-elasticsearch",
                bookDocumentMessage
            );

            CreateBookResponse response = new()
            {
                Id = book.Id,
                Name = book.Name,
                Author = book.Author,
                Publisher = book.Publisher,
                Description = book.Description,
                Price = book.Price,
                CategoryId = request.CategoryId,
                ImageUrls = imageUrls,
                Inventories = inventories.Select(inventory => new CreateBookInventoryResponse
                {
                    StoreId = inventory.StoreId,
                    Quantity = inventory.Quantity
                }).ToList()
            };

            return ApiResponse<object?>.Response(MessageRecord.CreatedSuccessfully, data: response);
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
    }
}
