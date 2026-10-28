using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Users.Requests;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGoogleService _googleService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UserService(
            IUserRepository userRepository, 
            IGoogleService googleService, 
            IMapper mapper,
            ICurrentUserService currentUserService
        )
        {
            _userRepository = userRepository;
            _googleService = googleService;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<object?>> CreateUserAsync(CreateUserRequest req, IFormFile? file)
        {
            // Case 1: User Exist
            // Case 2: Upload file
            // Case 2.1: Upload file failed
            // Case 2.2: Upload file success
            // Case 3: Save user to the database
            // Case 4: Rollback the avatar if saving to the database fails for the user.
            throw new BusinessException(errorRecord: ErrorRecord.Failed);
        }
    }
}
