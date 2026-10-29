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
                    messageRecord: MessageRecord.Success,
                    data: response
                );
            }
            throw new BusinessException(ErrorRecord.Failed);
        }
    }
}
