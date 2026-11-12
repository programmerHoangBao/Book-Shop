using back_end.Data;
using back_end.Entities;
using back_end.Enums;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repositories.Implements
{
    public class ReturnRequestRepository : IReturnRequestRepository
    {
        private readonly DatabaseContext _context;

        public ReturnRequestRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(CreateReturnRequestResult Result, ReturnRequestEntity? ReturnRequest)> CreateAsync(
            Guid userId,
            Guid orderId,
            ReturnRequestEntity returnRequest,
            DateTime requestedAt)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                OrderEntity? order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(candidate => candidate.Id == orderId &&
                        candidate.UserId == userId &&
                        !candidate.IsDeleted);
                if (order == null)
                {
                    await transaction.RollbackAsync();
                    return (CreateReturnRequestResult.OrderNotFound, null);
                }

                if (order.OrderStatus != OrderStatus.Completed)
                {
                    await transaction.RollbackAsync();
                    return (CreateReturnRequestResult.OrderNotCompleted, null);
                }

                if (!order.CompletedAt.HasValue || requestedAt > order.CompletedAt.Value.AddDays(3))
                {
                    await transaction.RollbackAsync();
                    return (CreateReturnRequestResult.ReturnWindowExpired, null);
                }

                if (await _context.ReturnRequests.AnyAsync(candidate => candidate.OrderId == orderId))
                {
                    await transaction.RollbackAsync();
                    return (CreateReturnRequestResult.AlreadyExists, null);
                }

                Dictionary<Guid, int> orderedQuantities = await _context.OrderItems
                    .Where(item => item.OrderId == orderId)
                    .ToDictionaryAsync(item => item.Id, item => item.Quantity);
                if (returnRequest.Items.Count == 0 ||
                    returnRequest.Items.Any(item =>
                        !orderedQuantities.TryGetValue(item.OrderItemId, out int orderedQuantity) ||
                        item.Quantity < 1 || item.Quantity > orderedQuantity) ||
                    returnRequest.Items.Select(item => item.OrderItemId).Distinct().Count() != returnRequest.Items.Count)
                {
                    await transaction.RollbackAsync();
                    return (CreateReturnRequestResult.InvalidItems, null);
                }

                returnRequest.UserId = userId;
                returnRequest.OrderId = orderId;
                returnRequest.Status = ReturnRequestStatus.Pending;
                returnRequest.RequestedAt = requestedAt;
                _context.ReturnRequests.Add(returnRequest);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CreateReturnRequestResult.Created, returnRequest);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                if (await _context.ReturnRequests.AnyAsync(candidate => candidate.OrderId == orderId))
                {
                    return (CreateReturnRequestResult.AlreadyExists, null);
                }

                throw;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
