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
        private readonly IStoreStaffRepository _storeStaffRepository;

        public ReturnRequestService(
            IReturnRequestRepository returnRequestRepository,
            ICurrentUserService currentUserService,
            IStoreStaffRepository storeStaffRepository)
        {
            _returnRequestRepository = returnRequestRepository;
            _currentUserService = currentUserService;
            _storeStaffRepository = storeStaffRepository;
        }

        public async Task<ApiResponse<PaginationResponse<ReturnRequestResponse>>> GetForCurrentCustomerAsync(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100 ||
                (long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue || userId.Value == Guid.Empty ||
                _currentUserService.GetRoleCurrentUser() != RoleUser.Customer)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            var result = await _returnRequestRepository.GetForCustomerAsync(
                userId.Value,
                pageNumber,
                pageSize);

            PaginationResponse<ReturnRequestResponse> page = new()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize),
                Items = result.ReturnRequests.Select(returnRequest => new ReturnRequestResponse
                {
                    Id = returnRequest.Id,
                    OrderId = returnRequest.OrderId,
                    OrderCode = returnRequest.Order?.OrderCode ?? string.Empty,
                    Status = returnRequest.Status,
                    Reason = returnRequest.Reason,
                    BankAccountName = returnRequest.BankAccountName,
                    BankAccountNumber = returnRequest.BankAccountNumber,
                    BankName = returnRequest.BankName,
                    RequestedAt = returnRequest.RequestedAt,
                    ApprovedAt = returnRequest.ApprovedAt,
                    RejectedAt = returnRequest.RejectedAt,
                    CompletedAt = returnRequest.CompletedAt,
                    Items = returnRequest.Items.Select(item => new ReturnRequestListItemResponse
                    {
                        OrderItemId = item.OrderItemId,
                        BookId = item.OrderItem?.BookId ?? Guid.Empty,
                        BookName = item.OrderItem?.Book?.Name ?? string.Empty,
                        Quantity = item.Quantity
                    }).ToList(),
                    EvidenceImageUrls = returnRequest.Evidences
                        .Select(evidence => evidence.ImageUrl)
                        .ToList()
                }).ToList()
            };

            return ApiResponse<PaginationResponse<ReturnRequestResponse>>.Response(
                MessageRecord.Success,
                data: page);
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

        public async Task<ApiResponse<object?>> ConfirmAsync(Guid returnRequestId)
        {
            if (returnRequestId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            RoleUser? role = _currentUserService.GetRoleCurrentUser();
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            Guid? storeId = null;
            if (role is RoleUser.Manager or RoleUser.Employee)
            {
                StoreEntity? store = await _storeStaffRepository.GetStoreByUserIdAsync(userId.Value);
                if (store == null || store.IsDeleted)
                {
                    throw new BusinessException(ErrorRecord.Forbidden);
                }

                storeId = store.Id;
            }
            else if (role != RoleUser.Admin)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            DateTime approvedAt = DateTime.UtcNow;
            var (result, returnRequest) = await _returnRequestRepository.ConfirmAsync(
                returnRequestId,
                storeId,
                approvedAt);

            switch (result)
            {
                case ConfirmReturnRequestResult.NotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case ConfirmReturnRequestResult.NotPending:
                    throw new BusinessException(ErrorRecord.ReturnRequestNotPending);
                case ConfirmReturnRequestResult.StoreNotAssociated:
                    throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (returnRequest == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            ConfirmReturnResponse response = new()
            {
                ReturnRequestId = returnRequest.Id,
                OrderId = returnRequest.OrderId,
                Status = returnRequest.Status,
                ApprovedAt = returnRequest.ApprovedAt ?? approvedAt,
                WasAlreadyApproved = result == ConfirmReturnRequestResult.AlreadyApproved
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }
    }
}
