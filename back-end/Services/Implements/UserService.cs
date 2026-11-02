using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Files.Responses;
using back_end.DTOs.Users.Infos;
using back_end.DTOs.Users.Requests;
using back_end.DTOs.Users.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Repositories;
using back_end.Settings;
using back_end.Utilities;
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly  IFileService _fileService;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreStaffRepository _staffRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly SecuritySetting _securitySetting;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IFileService fileService, 
            IStoreRepository storeRepository,
            IStoreStaffRepository staffRepository,
            ICurrentUserService currentUserService,
            IOptions<SecuritySetting> options,
            IKafkaProducer kafkaProducer,
            IMapper mapper
        )
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _storeRepository = storeRepository;
            _staffRepository = staffRepository;
            _currentUserService = currentUserService;
            _securitySetting = options.Value;
            _kafkaProducer = kafkaProducer;
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

            // Case 2: Check the user's permissions for creating users in detail and store not found
            if (req.Role != Enums.RoleUser.Customer && req.StoreId != Guid.Empty)
            {
                if (
                    _currentUserService.GetRoleCurrentUser() == Enums.RoleUser.Manager
                    && (req.Role == Enums.RoleUser.Admin || req.Role == Enums.RoleUser.Manager)
                    )
                {
                    throw new BusinessException(ErrorRecord.Forbidden);
                }

                StoreEntity? store = await _storeRepository.GetStoreByIdAsync(req.StoreId);
                if (store == null || store.IsDeleted)
                {
                    throw new BusinessException(ErrorRecord.StoreNotFound);
                }

            }
            // Case 3: Upload file
            if (file != null && file.Length > 0)
            {
                uploadImageResponse = await _fileService.UploadImageAsync(file);
            }
            // Case 4: Save user to the database
            UserEntity newUser = _mapper.Map<UserEntity>(req);
            newUser.AvatarUrl = uploadImageResponse?.ImageUrl ?? newUser.AvatarUrl;
            newUser.PasswordHash = HashUtility.HashBySHA256(req.Password, _securitySetting.SHASecrectKey);
            StoreStaffEntity? staff = null;
            bool addStaff = false;
            bool createdUser = await _userRepository.AddUserAsync(newUser);
            CreateUserResponse response = _mapper.Map<CreateUserResponse>(newUser);
            if (req.Role != Enums.RoleUser.Customer)
            {
                staff = new StoreStaffEntity
                {
                    StoreId = req.StoreId,
                    UserId = newUser.Id,
                };
                addStaff = await _staffRepository.AddStoreStaffAsync(staff);
                response.StoreId = staff.StoreId;
            }
            if (
                (createdUser && addStaff && req.Role != Enums.RoleUser.Customer) ||
                (createdUser && req.Role == Enums.RoleUser.Customer)
            )
            {
                return ApiResponse<object?>.Response(
                    MessageRecord.CreatedSuccessfully,
                    data: response
                );
            }
            else
            {
                // Case 5: Rollback the avatar if saving to the database fails for the user.
                if (uploadImageResponse != null)
                    await _fileService.DeleteImageAsync(uploadImageResponse.ObjectKey);

                if (!addStaff && staff != null)
                    await _staffRepository.RemoveStoreStaffAsync(staff);

                throw new BusinessException(errorRecord: ErrorRecord.Failed);
            }
        }

        public async Task<ApiResponse<object?>> GetProfileByIdAsync(Guid userId)
        {
            UserEntity? user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            RoleUser? currentRole = _currentUserService.GetRoleCurrentUser();
            Guid? currentUserId = _currentUserService.GetCurrentUserId();

            if (!CanViewUserProfile(currentRole, currentUserId, user))
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            GetProfileResponse response = _mapper.Map<GetProfileResponse>(user);

            if (user.Role != RoleUser.Customer && user.Role != RoleUser.Admin)
            {
                StoreEntity? store = await _staffRepository.GetStoreByUserIdAsync(user.Id);

                if (store == null || store.IsDeleted)
                {
                    throw new BusinessException(ErrorRecord.StoreNotFound);
                }

                response.StoreInfo = _mapper.Map<StoreInfo>(store);
            }

            return ApiResponse<object?>.Response(
                MessageRecord.Success,
                data: response
            );
        }

        public async Task<ApiResponse<object?>> GetProfileUserByEmailAsync(string email)
        {
            UserEntity? user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            RoleUser? currentRole = _currentUserService.GetRoleCurrentUser();
            Guid? currentUserId = _currentUserService.GetCurrentUserId();

            if (!CanViewUserProfile(currentRole, currentUserId, user))
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            GetProfileResponse response = _mapper.Map<GetProfileResponse>(user);

            if (user.Role != RoleUser.Customer && user.Role != RoleUser.Admin)
            {
                StoreEntity? store = await _staffRepository.GetStoreByUserIdAsync(user.Id);

                if (store == null || store.IsDeleted)
                {
                    throw new BusinessException(ErrorRecord.StoreNotFound);
                }

                response.StoreInfo = _mapper.Map<StoreInfo>(store);
            }

            return ApiResponse<object?>.Response(
                MessageRecord.Success,
                data: response
            );
        }

        public async Task<ApiResponse<object?>> UpdateUserAsync(UpdateUserRequest req, IFormFile? file)
        {
            UserEntity? existsUser = await _userRepository.GetUserByIdAsync(req.Id);
            UploadImageResponse? uploadImageResponse = null;
            RoleUser? currentRole = _currentUserService.GetRoleCurrentUser();
            Guid? currentUserId = _currentUserService.GetCurrentUserId();
            string oldAvatarUrl = existsUser?.AvatarUrl ?? string.Empty;
            if (existsUser == null || existsUser.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            if (!CanViewUserProfile(currentRole, currentUserId, existsUser))
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }
            if (file != null && file.Length > 0)
            {
                uploadImageResponse = await _fileService.UploadImageAsync(file);
            }
            existsUser = _mapper.Map(req, existsUser);
            if (uploadImageResponse != null)
            {
                existsUser.AvatarUrl = uploadImageResponse.ImageUrl;
            }
            bool updated = await _userRepository.UpdateUserAsync(existsUser);
            if (updated)
            {
                if (!String.IsNullOrEmpty(oldAvatarUrl) && uploadImageResponse != null)
                {
                    await _kafkaProducer.ProduceAsync(
                    "delete-file",
                    oldAvatarUrl);
                }
                return ApiResponse<object?>.Response(
                    MessageRecord.Success,
                    data: _mapper.Map<UpdateUserResponse>(existsUser)
                );
            }
            else
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
        }

        private bool CanViewUserProfile(RoleUser? currentRole, Guid? currentUserId, UserEntity targetUser)
        {
            // Admin can view all profiles
            if (currentRole == RoleUser.Admin)
            {
                return true;
            }

            // Users can view their own profiles
            if (currentUserId == targetUser.Id)
            {
                return true;
            }

            return currentRole switch
            {
                // Manager can view Employee and Customer
                RoleUser.Manager =>
                    targetUser.Role == RoleUser.Employee ||
                    targetUser.Role == RoleUser.Customer,

                // Employee can view Customer
                RoleUser.Employee =>
                    targetUser.Role == RoleUser.Customer,

                // Customer cannot view other users
                RoleUser.Customer => false,

                _ => false
            };
        }
    }
}
