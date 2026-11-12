using back_end.DTOs;
using back_end.DTOs.Returns.Requests;
using back_end.DTOs.Returns.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class ReturnRequestService : IReturnRequestService
    {
        private const int MaximumEvidenceCount = 10;
        private const int MaximumEvidenceUrlLength = 2048;
        private readonly IReturnRequestRepository _returnRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReturnRequestService(
            IReturnRequestRepository returnRequestRepository,
            ICurrentUserService currentUserService)
        {
            _returnRequestRepository = returnRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<object?>> CreateAsync(
            Guid orderId,
            CreateReturnRequest request)
        {
            if (orderId == Guid.Empty || request == null ||
                string.IsNullOrWhiteSpace(request.BankAccountName) ||
                request.BankAccountName.Trim().Length > 255 ||
                string.IsNullOrWhiteSpace(request.BankAccountNumber) ||
                request.BankAccountNumber.Trim().Length > 30 ||
                string.IsNullOrWhiteSpace(request.BankName) ||
                request.BankName.Trim().Length > 255 ||
                string.IsNullOrWhiteSpace(request.Reason) ||
                request.Reason.Trim().Length > 1000 ||
                request.Items == null || request.Items.Count == 0 ||
                request.EvidenceImageUrls == null || request.EvidenceImageUrls.Count == 0 ||
                request.EvidenceImageUrls.Count > MaximumEvidenceCount ||
                request.EvidenceImageUrls.Any(url =>
                    string.IsNullOrWhiteSpace(url) || url.Trim().Length > MaximumEvidenceUrlLength))
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue || userId.Value == Guid.Empty ||
                _currentUserService.GetRoleCurrentUser() != RoleUser.Customer)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            DateTime requestedAt = DateTime.UtcNow;
            ReturnRequestEntity returnRequest = new()
            {
                BankAccountName = request.BankAccountName.Trim(),
                BankAccountNumber = request.BankAccountNumber.Trim(),
                BankName = request.BankName.Trim(),
                Reason = request.Reason.Trim(),
                Items = request.Items.Select(item => new ReturnItemEntity
                {
                    OrderItemId = item.OrderItemId,
                    Quantity = item.Quantity
                }).ToList(),
                Evidences = request.EvidenceImageUrls.Select(url => new ReturnRequestEvidenceEntity
                {
                    ImageUrl = url.Trim()
                }).ToList()
            };

            var (result, createdRequest) = await _returnRequestRepository.CreateAsync(
                userId.Value,
                orderId,
                returnRequest,
                requestedAt);

            switch (result)
            {
                case CreateReturnRequestResult.OrderNotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case CreateReturnRequestResult.OrderNotCompleted:
                    throw new BusinessException(ErrorRecord.ReturnOrderNotCompleted);
                case CreateReturnRequestResult.ReturnWindowExpired:
                    throw new BusinessException(ErrorRecord.ReturnWindowExpired);
                case CreateReturnRequestResult.AlreadyExists:
                    throw new BusinessException(ErrorRecord.ReturnRequestAlreadyExists);
                case CreateReturnRequestResult.InvalidItems:
                    throw new BusinessException(ErrorRecord.ReturnItemsInvalid);
            }

            if (createdRequest == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            CreateReturnRequestResponse response = new()
            {
                ReturnRequestId = createdRequest.Id,
                OrderId = createdRequest.OrderId,
                Status = createdRequest.Status,
                RequestedAt = createdRequest.RequestedAt
            };

            return ApiResponse<object?>.Response(MessageRecord.CreatedSuccessfully, data: response);
        }
    }
}
