using back_end.Data;
using back_end.DTOs.Carts.Responses;
using back_end.Entities;
using back_end.Enums;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly DatabaseContext _context;

        public CartRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(AddBookToCartResult Result, CartItemResponse? Item)> AddBookAsync(
            Guid userId,
            Guid bookId,
            int quantity)
        {
            BookEntity? book = await _context.Books
                .FirstOrDefaultAsync(book => book.Id == bookId && !book.IsDeleted);

            if (book == null)
            {
                return (AddBookToCartResult.BookNotFound, null);
            }

            if (book.Status != BookStatus.Available)
            {
                return (AddBookToCartResult.BookUnavailable, null);
            }

            int availableQuantity = await _context.Inventories
                .Where(inventory => inventory.BookId == bookId && !inventory.IsDeleted)
                .SumAsync(inventory => (int?)inventory.AvailableQuantity) ?? 0;

            CartEntity? cart = await _context.Carts
                .FirstOrDefaultAsync(cart => cart.UserId == userId);

            if (cart == null)
            {
                cart = new CartEntity { UserId = userId };
                await _context.Carts.AddAsync(cart);
            }
            else if (cart.IsDeleted)
            {
                cart.IsDeleted = false;
            }

            CartItemEntity? cartItem = cart.Id == Guid.Empty
                ? null
                : await _context.CartItems.FirstOrDefaultAsync(item =>
                    item.CartId == cart.Id && item.BookId == bookId && !item.IsDeleted);

            int updatedQuantity = (cartItem?.Quantity ?? 0) + quantity;
            if (updatedQuantity > availableQuantity)
            {
                return (AddBookToCartResult.InsufficientStock, null);
            }

            if (cartItem == null)
            {
                cartItem = new CartItemEntity
                {
                    Cart = cart,
                    BookId = bookId,
                    Quantity = quantity
                };
                await _context.CartItems.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity = updatedQuantity;
            }

            if (await _context.SaveChangesAsync() == 0)
            {
                return (AddBookToCartResult.Success, null);
            }

            return (AddBookToCartResult.Success, new CartItemResponse
            {
                BookId = book.Id,
                BookName = book.Name,
                Quantity = cartItem.Quantity,
                UnitPrice = book.Price
            });
        }
    }
}
