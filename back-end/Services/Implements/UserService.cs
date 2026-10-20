using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Google.Responses;
using back_end.DTOs.Users.Requests;
using back_end.DTOs.Users.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            UserEntity? existUser = await _userRepository.GetUserByEmailAsync(req.Email);
            string? avataUrl = null;
            string? uploadedFileId = null;
            
            // Case 1: User exists!
            if (existUser != null)
            {
                throw new BusinessException(ErrorRecord.UserExists);
            }

            // Case 2: Upload file avata!
            if (file != null && file.Length > 0)
            {
                // Case 2.1: Upload file failed!
                ApiResponse<GoogleDriveFileResponse?> fileResponse = await _googleService.UploadImageAsync(file);
                if (!fileResponse.IsSuccess || fileResponse.Data == null)
                {
                    return new ApiResponse<object?>
                    {
                        IsSuccess = fileResponse.IsSuccess,
                        ResponseCode = fileResponse.ResponseCode,
                        Message = fileResponse.Message,
                        HttpStatus = fileResponse.HttpStatus,
                        Data = fileResponse.Data
                    };
                }
                // Case 2.2: Upload file success!
                uploadedFileId = fileResponse.Data.FileId;
                avataUrl = fileResponse.Data.WebViewLink;
            }

            // Case 3: Save user in database
            UserEntity newUser = _mapper.Map<UserEntity>(req);
            newUser.AvatarUrl = avataUrl;
            bool created = await _userRepository.AddUserAsync(newUser);
            if (created)
            {
                CreateUserResponse createUserResponse = 
                    _mapper.Map<CreateUserResponse>(newUser);
                return ApiResponse<object?>.Response(
                    messageRecord:MessageRecord.Success,
                    data:createUserResponse
                );
            }

            //Case 4: Roleback if save user in database failed!
            bool isRollback = false;
            if (!string.IsNullOrWhiteSpace(uploadedFileId))
            {
                isRollback = await _googleService.DeleteImageAsync(uploadedFileId);
            }
            if (!isRollback) // Case 5: Rolleback failed
            {
                throw new BusinessException(
                    errorRecord: ErrorRecord.UserCreationFailedWithAvatarCleanupFailed
                 );
            }

            // case 6: rolleback success and create user failed!
            throw new BusinessException(errorRecord: ErrorRecord.Failed);
        }
    }
}
