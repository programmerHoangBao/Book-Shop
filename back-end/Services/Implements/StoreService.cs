using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Stores.Requests;
using back_end.DTOs.Stores.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public StoreService(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object?>> CreateStoreAsync(CreateStoreRequest req)
        {
            StoreEntity newStore = _mapper.Map<StoreEntity>(req);
            bool created = await _storeRepository.AddStoreAsync(newStore);
            if (created)
            {
                CreateStoreResponse response = _mapper.Map<CreateStoreResponse>(newStore);
                return ApiResponse<object?>.Response(
                    messageRecord: MessageRecord.CreatedSuccessfully,
                    data: response
                );
            }
            throw new BusinessException(ErrorRecord.Failed);
        }

        public async Task<ApiResponse<object?>> GetStoresAsync(int pageNumber, int pageSize)
        {
            var result = await _storeRepository.GetStoresAsync(pageNumber, pageSize);
            List<StoreEntity> stores = result.Stores;
            int totalItems = result.TotalItems;
            if (totalItems == 0)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            List<StoreResponse> responses = stores
                .Select(s => _mapper.Map<StoreResponse>(s))
                .ToList();
            int totalPages = (int)Math.Ceiling(
                (double)totalItems / pageSize
            );
            PaginationResponse<StoreResponse> pagination = new()
            {
                Items = responses,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
            return ApiResponse<object?>.Response(MessageRecord.Success, data: pagination);
        }

        public async Task<ApiResponse<object?>> UpdateStoreAsync(UpdateStoreRequest req)
        {
            StoreEntity? existsStore = await _storeRepository.GetStoreByIdAsync(req.StoreId);
            if (existsStore == null || existsStore.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            existsStore = _mapper.Map(req, existsStore);
            bool updated = await _storeRepository.UpdateStoreAsync(existsStore);
            if (updated)
            {
                UpdateStoreResponse response = _mapper.Map<UpdateStoreResponse>(existsStore);
                return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
            }
            throw new BusinessException(ErrorRecord.Failed);
        }
    }
}
