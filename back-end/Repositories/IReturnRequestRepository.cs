using back_end.Entities;

namespace back_end.Repositories
{
    public enum CreateReturnRequestResult
    {
        Created,
        OrderNotFound,
        OrderNotCompleted,
        ReturnWindowExpired,
        AlreadyExists,
        InvalidItems
    }

    public enum ConfirmReturnRequestResult
    {
        Approved,
        AlreadyApproved,
        NotFound,
        NotPending,
        StoreNotAssociated
    }

    public interface IReturnRequestRepository
    {
        Task<(CreateReturnRequestResult Result, ReturnRequestEntity? ReturnRequest)> CreateAsync(
            Guid userId,
            Guid orderId,
            ReturnRequestEntity returnRequest,
            DateTime requestedAt);
        Task<(ConfirmReturnRequestResult Result, ReturnRequestEntity? ReturnRequest)> ConfirmAsync(
            Guid returnRequestId,
            Guid? storeId,
            DateTime approvedAt);
    }
}
