using back_end.DTOs;
using back_end.DTOs.Auths.Requests;
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
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly SecuritySetting _securitySetting;
        private readonly IRedisService _redisService;
        private readonly IKafkaProducer _kafkaProducer;

        public AuthService
        (
            IUserRepository userRepository, 
            IOptions<SecuritySetting> options,
            IRedisService redisService,
            IKafkaProducer kafkaProducer
        )
        {
            _userRepository = userRepository;
            _securitySetting = options.Value;
            _redisService = redisService;
            _kafkaProducer = kafkaProducer;
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
            string hashedPassword = HashUtility.HashBySHA256(req.Password, _securitySetting.SHASecrectKey); //Nhanh
            PendingRegistrationModel pendingRegistration = new PendingRegistrationModel
            {
                FullName = req.FullName,
                Email = req.Email,
                HashedPassword = hashedPassword,
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
    }
}
