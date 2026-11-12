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

        public async Task<(ConfirmReturnRequestResult Result, ReturnRequestEntity? ReturnRequest)> ConfirmAsync(
            Guid returnRequestId,
            Guid? storeId,
            DateTime approvedAt)
        {
            ReturnRequestEntity? returnRequest = await _context.ReturnRequests
                .AsNoTracking()
                .Include(candidate => candidate.Order)
                .FirstOrDefaultAsync(candidate => candidate.Id == returnRequestId);
            if (returnRequest == null || returnRequest.Order == null || returnRequest.Order.IsDeleted)
            {
                return (ConfirmReturnRequestResult.NotFound, null);
            }

            if (storeId.HasValue)
            {
                string reservationReason = $"Reserved for order {returnRequest.Order.OrderCode}";
                bool returnBelongsToStore = await _context.InventoryTransactions.AnyAsync(inventoryTransaction =>
                    inventoryTransaction.Reason == reservationReason &&
                    !inventoryTransaction.IsDeleted &&
                    inventoryTransaction.Inventory != null &&
                    inventoryTransaction.Inventory.StoreId == storeId.Value);
                if (!returnBelongsToStore)
                {
                    return (ConfirmReturnRequestResult.StoreNotAssociated, null);
                }
            }

            int updatedRows = await _context.ReturnRequests
                .Where(candidate => candidate.Id == returnRequestId &&
                    candidate.Status == ReturnRequestStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.Status, ReturnRequestStatus.Approved)
                    .SetProperty(candidate => candidate.ApprovedAt, approvedAt));

            if (updatedRows == 1)
            {
                returnRequest.Status = ReturnRequestStatus.Approved;
                returnRequest.ApprovedAt = approvedAt;
                return (ConfirmReturnRequestResult.Approved, returnRequest);
            }

            returnRequest = await _context.ReturnRequests
                .AsNoTracking()
                .Include(candidate => candidate.Order)
                .FirstOrDefaultAsync(candidate => candidate.Id == returnRequestId);
            if (returnRequest?.Status == ReturnRequestStatus.Approved)
            {
                return (ConfirmReturnRequestResult.AlreadyApproved, returnRequest);
            }

            return returnRequest == null
                ? (ConfirmReturnRequestResult.NotFound, null)
                : (ConfirmReturnRequestResult.NotPending, returnRequest);
        }
    }
}
