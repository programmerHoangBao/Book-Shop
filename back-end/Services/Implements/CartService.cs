using back_end.DTOs;
using back_end.DTOs.Carts.Requests;
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
    }
}
