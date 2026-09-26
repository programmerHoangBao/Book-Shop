using back_end.DTOs;
using back_end.DTOs.Auths.Requests;
using back_end.DTOs.Auths.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Redis;
using back_end.Redis.Models;
using back_end.Repositories;
using back_end.Settings;
using back_end.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly SecuritySetting _securitySetting;
        private readonly IRedisService _redisService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IJwtService _jwtService;

        public AuthService
        (
            IUserRepository userRepository, 
            IOptions<SecuritySetting> options,
            IRedisService redisService,
            IKafkaProducer kafkaProducer,
            IJwtService jwtService
        )
        {
            _userRepository = userRepository;
            _securitySetting = options.Value;
            _redisService = redisService;
            _kafkaProducer = kafkaProducer;
            _jwtService = jwtService;
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

        public async Task<ApiResponse<object?>> VerifyOtpAsync(VerifyOtpRequest req)
        {
            // Case: 1 User exists
            UserEntity? exitsUser = await _userRepository.GetUserByEmailAsync(req.Email);
            if (exitsUser != null)
            {
                throw new BusinessException(ErrorRecord.UserExists);
            }
            // Case 2: Otp is expired
            var key = $"pending-registration:{req.Email}";
            var pendingRegistration = await _redisService.GetAsync<PendingRegistrationModel>(key);
            if (pendingRegistration == null)
            {
                throw new BusinessException(ErrorRecord.OtpExpiry);
            }
            // Case 3: Otp is invalid
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
            // Case 4: Successfully
            return ApiResponse<object?>.Response(
                messageRecord: MessageRecord.Success    
            );
        }
    }
}
