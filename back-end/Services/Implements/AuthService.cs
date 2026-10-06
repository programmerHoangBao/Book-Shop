using back_end.DTOs;
using back_end.DTOs.Auths.Requests;
using back_end.DTOs.Auths.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Redis.Models;
using back_end.Redis.Services;
using back_end.Repositories;
using back_end.Settings;
using back_end.Utilities;
using Microsoft.Extensions.Options;
using static Google.Apis.Requests.BatchRequest;

namespace back_end.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly SecuritySetting _securitySetting;
        private readonly IRedisService _redisService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IJwtService _jwtService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<AuthService> _logger;

        public AuthService
        (
            IUserRepository userRepository, 
            IOptions<SecuritySetting> options,
            IRedisService redisService,
            IKafkaProducer kafkaProducer,
            IJwtService jwtService,
            IGoogleAuthService googleAuthService,
            ILogger<AuthService> logger
        )
        {
            _userRepository = userRepository;
            _securitySetting = options.Value;
            _redisService = redisService;
            _kafkaProducer = kafkaProducer;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        public async Task<ApiResponse<object?>> ForgotPasswordAsync(ForgotPasswordRequest req)
        {
            // Case 1: Find user
            UserEntity? user = await _userRepository.GetUserByEmailAsync(req.Email);
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            // Case 2: User not log in locally
            if (user.AuthProvider != Enums.AuthProvider.Local)
            {
                throw new BusinessException(ErrorRecord.UserNotLoggedInLocally);
            }
            string otp = OtpUtility.Generate(len: 6);
            var key = $"pending-forgot-password:{user.Email}";
            PendingForgotPasswordModel pendingForgotPassword = new PendingForgotPasswordModel
            {
                UserId = user.Id,
                Otp = otp,
            };
            await _redisService.SaveAsync(
                key,
                pendingForgotPassword,
                TimeSpan.FromSeconds(_securitySetting.OtpExpirySeconds)
            );
            var message = new SendOtpEmailMessage
            {
                ToEmail = user.Email,
                Otp = otp,
                Name = user.FullName,
                OtpExpirySeconds = _securitySetting.OtpExpirySeconds,
            };
            await _kafkaProducer.ProduceAsync(
                "send-otp-email",
                message
            );
            return ApiResponse<object?>.Response(MessageRecord.Success);
        }

        public async Task<ApiResponse<LoginResponse?>> GoogleSignInAsync(string idToken)
        {
            GoogleUserInfoResponse? googleUser =
                await _googleAuthService.VerifyTokenAsync(idToken);
            // Case 1: Id token invalid!
            if (googleUser == null)
            {
                _logger.LogError("id token invalid!");
                throw new BusinessException(ErrorRecord.Failed);
            }
            // Case 2: User not found
            UserEntity? user = await _userRepository.GetUserByEmailAsync(googleUser.Email);
            if (user == null)
            {
                user = new UserEntity
                {
                    Email = googleUser.Email,
                    FullName = googleUser.Name,
                    AvatarUrl = googleUser.AvatarUrl,
                    AuthProvider = Enums.AuthProvider.Google,
                };
                bool createdUser = await _userRepository.AddUserAsync(user);
                if (!createdUser)
                {
                    throw new BusinessException(ErrorRecord.Failed);
                }
            }
            else if (user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            else if (user.AuthProvider == Enums.AuthProvider.Local)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            string accessToken = _jwtService.GenerateAccessToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken();
            string refreshTokenHash = HashUtility.HashBySHA256(refreshToken, _securitySetting.SHASecrectKey);
            var redisKey = $"refresh_token:{refreshTokenHash}";
            RefreshTokenModel refreshTokenModel = new RefreshTokenModel
            {
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
            };
            await _redisService.SaveAsync(
                redisKey,
                refreshTokenModel,
                TimeSpan.FromDays(_securitySetting.RefreshTokenExpirationDays)
            );
            LoginResponse response = new LoginResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            return ApiResponse<LoginResponse?>.Response(
                messageRecord: MessageRecord.Success,
                data: response
            );
        }

        public async Task<ApiResponse<LoginResponse?>> LoginAsync(LoginRequest req)
        {
            // Case 1: Find user
            UserEntity? user = await _userRepository.GetUserByEmailAsync(req.Email);
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            // Case 2: Do not log in locally!
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new BusinessException(ErrorRecord.LoginFailed);
            }
            // Case 3: Incorrect Password
            bool validPassword = HashUtility.Verify(
                input: req.Password,
                hashedInput: user.PasswordHash,
                isBCrypt: false,
                shaSecrectKey: _securitySetting.SHASecrectKey
            );
            if (!validPassword)
            {
                throw new BusinessException(ErrorRecord.LoginFailed);
            }
            // Case 5: Login success
            string accessToken = _jwtService.GenerateAccessToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken();
            string refreshTokenHash = HashUtility.HashBySHA256(refreshToken, _securitySetting.SHASecrectKey);
            var redisKey = $"refresh_token:{refreshTokenHash}";
            RefreshTokenModel refreshTokenModel = new RefreshTokenModel
            {
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
            };
            await _redisService.SaveAsync(
                redisKey,
                refreshTokenModel,
                TimeSpan.FromDays(_securitySetting.RefreshTokenExpirationDays)
            );
            LoginResponse response = new LoginResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            return ApiResponse<LoginResponse?>.Response(
                messageRecord: MessageRecord.Success,
                data: response
            );
        }

        public async Task<ApiResponse<object?>> RefreshTokenAsync(RefreshTokenRequest req)
        {
            // Case 1: RefreshToken expired
            string refreshTokenHash = HashUtility.HashBySHA256(req.RefreshToken, _securitySetting.SHASecrectKey);
            var redisKey = $"refresh_token:{refreshTokenHash}";
            var refreshTokenModel = await _redisService.GetAsync<RefreshTokenModel>(redisKey);
            if ( refreshTokenModel == null )
            {
                throw new BusinessException(ErrorRecord.RefreshTokenExpiry);
            }
            // Case 2: RefreshToken invalied
            if (refreshTokenModel.RefreshTokenHash != refreshTokenHash)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            // Case 3: Successful
            await _redisService.DeleteAsync(redisKey);
            UserEntity? user = await _userRepository.GetUserByIdAsync(refreshTokenModel.UserId);
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            string newAccessToken = _jwtService.GenerateAccessToken(user);
            string newRefreshToken = _jwtService.GenerateRefreshToken();
            string newRefreshTokenHash = HashUtility.HashBySHA256(newRefreshToken, _securitySetting.SHASecrectKey);
            var newRedisKey = $"refresh_token:{newRefreshTokenHash}";
            RefreshTokenModel newRefreshTokenModel = new RefreshTokenModel
            {
                UserId = user.Id,
                RefreshTokenHash = newRefreshTokenHash,
            };
            await _redisService.SaveAsync(
                newRedisKey,
                newRefreshTokenModel,
                TimeSpan.FromDays(_securitySetting.RefreshTokenExpirationDays)
            );
            LoginResponse response = new LoginResponse
            {
                UserId = user.Id,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
            return ApiResponse<object?>.Response(
                messageRecord: MessageRecord.Success,
                data: response
            );
        }

        public async Task<ApiResponse<object?>> RegisterAsync(RegisterRequest req)
        {
            UserEntity? user = await _userRepository.GetUserByEmailAsync(req.Email);
            if (user != null)
            {
                throw new BusinessException(ErrorRecord.UserExists);
            }
            string otp = OtpUtility.Generate(len: 6);
            //string hashedPassword = HashUtility.HashByBCrypt(req.Password, 12); // Chậm
            string passwordHash = HashUtility.HashBySHA256(req.Password, _securitySetting.SHASecrectKey); //Nhanh
            PendingRegistrationModel pendingRegistration = new PendingRegistrationModel
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = passwordHash,
                Otp = otp,
            };
            var key = $"pending-registration:{pendingRegistration.Email}";
            await _redisService.SaveAsync(
                key, 
                pendingRegistration, 
                TimeSpan.FromSeconds(_securitySetting.OtpExpirySeconds)
            );
            var message = new SendOtpEmailMessage
            {
                ToEmail = req.Email,
                Otp = otp,
                Name = req.FullName,
                OtpExpirySeconds = _securitySetting.OtpExpirySeconds,
            };
            await _kafkaProducer.ProduceAsync(
                "send-otp-email",
                message
            );
            return ApiResponse<object?>.Response(MessageRecord.RegisterSuccessfully);
        }

        public async Task<ApiResponse<object?>> ResetPasswordAsync(ResetPasswordRequest req)
        {
            var resetPasswordKey = $"reset-password:{req.ResetPasswordKey}";
            var resetPasswordModel = await _redisService.GetAsync<ResetPasswordModel>(resetPasswordKey);
            // Case 1: Reset password key is expired
            if (resetPasswordModel == null)
            {
                throw new BusinessException(ErrorRecord.ResetPasswordKeyExpiry);
            }
            // Case 2: Reset password key is invalid
            if (resetPasswordModel.ResetPasswordKey != req.ResetPasswordKey)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            UserEntity? user = await _userRepository.GetUserByIdAsync(resetPasswordModel.UserId);
            // Case 3: User not found
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }
            // Case 4: User not log in locally
            if (user.AuthProvider != Enums.AuthProvider.Local)
            {
                throw new BusinessException(ErrorRecord.UserNotLoggedInLocally);
            }
            // Case 5: Reset password successfully
            string newPasswordHash = HashUtility.HashBySHA256(req.NewPassword, _securitySetting.SHASecrectKey);
            user.PasswordHash = newPasswordHash;
            bool updateUser = await _userRepository.UpdateUserAsync(user);
            if (!updateUser)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            await _redisService.DeleteAsync(resetPasswordKey);
            return ApiResponse<object?>.Response(
                messageRecord: MessageRecord.Success
            );
        }

        public async Task<ApiResponse<object?>> VerifyOtpAsync(VerifyOtpRequest req)
        {
            UserEntity? exitsUser = await _userRepository.GetUserByEmailAsync(req.Email);
            if (exitsUser != null)
            {
                return await VerifyOtpForgotPassword(req);
            }
            return await VerifyOtpRegistrationAsync(req);
        }
        private async Task<ApiResponse<object?>> VerifyOtpForgotPassword(VerifyOtpRequest req)
        {
            var key = $"pending-forgot-password:{req.Email}";
            var pendingForgotPassword = _redisService.GetAsync<PendingForgotPasswordModel>(key).Result;
            // Case 1: Otp is expired
            if (pendingForgotPassword == null)
            {
                throw new BusinessException(ErrorRecord.OtpExpiry);
            }
            // Case 2: Otp is invalid
            if (req.Otp != pendingForgotPassword.Otp)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            // Case 3: Successfully
            VerifyOtpResponse response = new VerifyOtpResponse
            {
                ResetPasswordKey = Guid.NewGuid(),
                Action = Enums.AuthAction.ForgotPassword
            };
            var resetPasswordKey = $"reset-password:{response.ResetPasswordKey}";
            var resetPasswordModel = new ResetPasswordModel
            {
                ResetPasswordKey = response.ResetPasswordKey,
                UserId = pendingForgotPassword.UserId
            };
            await _redisService.SaveAsync<ResetPasswordModel>(
                resetPasswordKey,
                resetPasswordModel,
                TimeSpan.FromMinutes(_securitySetting.ResetPasswordExpiryMinutes)
            );
            await _redisService.DeleteAsync(key);
            return ApiResponse<object?>.Response(
                messageRecord: MessageRecord.Success,
                data: response
            );
        }
        private async Task<ApiResponse<object?>> VerifyOtpRegistrationAsync(VerifyOtpRequest req)
        {
            // Case 1: Otp is expired
            var key = $"pending-registration:{req.Email}";
            var pendingRegistration = await _redisService.GetAsync<PendingRegistrationModel>(key);
            if (pendingRegistration == null)
            {
                throw new BusinessException(ErrorRecord.OtpExpiry);
            }
            // Case 2: Otp is invalid
            if (req.Otp != pendingRegistration.Otp)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            UserEntity newUser = new UserEntity
            {
                FullName = pendingRegistration.FullName,
                Email = req.Email,
                PasswordHash = pendingRegistration.PasswordHash
            };
            bool saveUser = await _userRepository.AddUserAsync(newUser);
            if (!saveUser)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }
            await _redisService.DeleteAsync(key);
            // Case 3: Successfully
            return ApiResponse<object?>.Response(
                messageRecord: MessageRecord.Success
            );
        }
    }
}
