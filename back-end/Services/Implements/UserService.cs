using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Files.Responses;
using back_end.DTOs.Users.Requests;
using back_end.DTOs.Users.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly  IFileService _fileService;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IFileService fileService, 
            IMapper mapper
        )
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object?>> CreateUserAsync(CreateUserRequest req, IFormFile? file)
        {
            UserEntity? existsUser = await _userRepository.GetUserByEmailAsync(req.Email);
            UploadImageResponse? uploadImageResponse = null;
            // Case 1: User Exist
            if (existsUser != null)
            {
                throw new BusinessException(ErrorRecord.UserExists);
            }
            // Case 2: Upload file
            if (file != null && file.Length > 0)
            {
                uploadImageResponse = await _fileService.UploadImageAsync(file);
            }
            // Case 3: Save user to the database
            UserEntity newUser = _mapper.Map<UserEntity>(req);
            newUser.AvatarUrl = uploadImageResponse?.ImageUrl ?? newUser.AvatarUrl;
            bool created = await _userRepository.AddUserAsync(newUser);
            if (created)
            {
                CreateUserResponse response = _mapper.Map<CreateUserResponse>(newUser);
                return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
            }
            // Case 4: Rollback the avatar if saving to the database fails for the user.

            if (uploadImageResponse != null)
            {
                await _fileService.DeleteImageAsync(uploadImageResponse.ObjectKey);
            }
            throw new BusinessException(errorRecord: ErrorRecord.Failed);
        }
    }
}
