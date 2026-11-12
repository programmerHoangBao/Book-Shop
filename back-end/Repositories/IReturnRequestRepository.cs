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

    public interface IReturnRequestRepository
    {
        Task<(CreateReturnRequestResult Result, ReturnRequestEntity? ReturnRequest)> CreateAsync(
            Guid userId,
            Guid orderId,
            ReturnRequestEntity returnRequest,
            DateTime requestedAt);
    }
}
