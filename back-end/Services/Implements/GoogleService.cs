using back_end.DTOs.Google.Responses;
using back_end.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class GoogleService : IGoogleService
    {
        private readonly GoogleSetting _googleSetting;
        private readonly ILogger<GoogleService> _logger;

        public GoogleService(
            IOptions<GoogleSetting> options,
            ILogger<GoogleService> logger)
        {
            _googleSetting = options.Value;
            _logger = logger;
        }

        public async Task<GoogleUserInfoResponse?> VerifyTokenAsync(
            string idToken)
        {
            try
            {
                GoogleJsonWebSignature.Payload payload =
                    await GoogleJsonWebSignature.ValidateAsync(
                        idToken,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[]
                            {
                                _googleSetting.ClientId
                            }
                        });

                return new GoogleUserInfoResponse
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    AvatarUrl = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch
            {
                return null;
            }
        }
    }
}