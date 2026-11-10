using back_end.DTOs;
using back_end.DTOs.Carts.Requests;
using back_end.DTOs.Carts.Responses;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public CartService(ICartRepository cartRepository, ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PaginationResponse<CartItemResponse>>> GetBooksAsync(
            int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            var result = await _cartRepository.GetBooksAsync(
                userId.Value, pageNumber, pageSize);

            PaginationResponse<CartItemResponse> response = new()
            {
                Items = result.Items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize)
            };

            return ApiResponse<PaginationResponse<CartItemResponse>>.Response(
                MessageRecord.Success, data: response);
        }

        public async Task<ApiResponse<object?>> AddBookAsync(AddBookToCartRequest request)
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (request.BookId == Guid.Empty || request.Quantity < 1)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var (result, item) = await _cartRepository.AddBookAsync(
                userId.Value,
                request.BookId,
                request.Quantity);

            switch (result)
            {
                case AddBookToCartResult.BookNotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case AddBookToCartResult.BookUnavailable:
                    throw new BusinessException(ErrorRecord.BookUnavailable);
                case AddBookToCartResult.InsufficientStock:
                    throw new BusinessException(ErrorRecord.InsufficientStock);
            }

            if (item == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            return ApiResponse<object?>.Response(MessageRecord.Success, data: item);
        }

        public async Task<ApiResponse<object?>> RemoveBookAsync(Guid bookId)
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (bookId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if (!await _cartRepository.RemoveBookAsync(userId.Value, bookId))
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            return ApiResponse<object?>.Response(MessageRecord.Success);
        }
    }
}
