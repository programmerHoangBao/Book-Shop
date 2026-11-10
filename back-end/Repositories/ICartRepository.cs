using back_end.DTOs.Carts.Responses;

namespace back_end.Repositories
{
    public enum AddBookToCartResult
    {
        Success,
        BookNotFound,
        BookUnavailable,
        InsufficientStock
    }

    public interface ICartRepository
    {
        Task<(AddBookToCartResult Result, CartItemResponse? Item)> AddBookAsync(
            Guid userId,
            Guid bookId,
            int quantity);
        Task<bool> RemoveBookAsync(Guid userId, Guid bookId);
    }
}
