using back_end.DTOs;
using back_end.DTOs.Stores.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class StoreInventoryService : IStoreInventoryService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreStaffRepository _storeStaffRepository;
        private readonly ICurrentUserService _currentUserService;

        public StoreInventoryService(
            IBookRepository bookRepository,
            IStoreRepository storeRepository,
            IStoreStaffRepository storeStaffRepository,
            ICurrentUserService currentUserService)
        {
            _bookRepository = bookRepository;
            _storeRepository = storeRepository;
            _storeStaffRepository = storeStaffRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PaginationResponse<StoreInventoryBookResponse>>> GetStoreBooksAsync(
            Guid storeId,
            int pageNumber,
            int pageSize)
        {
            if (storeId == Guid.Empty || pageNumber < 1 || pageSize < 1 || pageSize > 100 ||
                (long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            StoreEntity? store = await _storeRepository.GetStoreByIdAsync(storeId);
            if (store == null || store.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.StoreNotFound);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            RoleUser? role = _currentUserService.GetRoleCurrentUser();
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (role != RoleUser.Admin)
            {
                if (role is not (RoleUser.Manager or RoleUser.Employee) ||
                    !await _storeStaffRepository.IsUserAssignedToStoreAsync(userId.Value, storeId))
                {
                    throw new BusinessException(ErrorRecord.Forbidden);
                }
            }

            var result = await _bookRepository.GetStoreInventoryBooksAsync(
                storeId,
                pageNumber,
                pageSize);

            PaginationResponse<StoreInventoryBookResponse> page = new()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize),
                Items = result.Inventories
                    .Where(inventory => inventory.Book != null)
                    .Select(inventory => new StoreInventoryBookResponse
                    {
                        BookId = inventory.BookId,
                        Name = inventory.Book!.Name,
                        Author = inventory.Book.Author,
                        Publisher = inventory.Book.Publisher,
                        Price = inventory.Book.Price,
                        Status = inventory.Book.Status,
                        Quantity = inventory.Quantity,
                        ReservedQuantity = inventory.ReservedQuantity,
                        AvailableQuantity = inventory.AvailableQuantity,
                        DamagedQuantity = inventory.DamagedQuantity,
                        IncomingQuantity = inventory.IncomingQuantity,
                        ImageUrls = inventory.Book.Images.Select(image => image.ImageUrl).ToList()
                    })
                    .ToList()
            };

            return ApiResponse<PaginationResponse<StoreInventoryBookResponse>>.Response(
                MessageRecord.Success,
                data: page);
        }
    }
}
