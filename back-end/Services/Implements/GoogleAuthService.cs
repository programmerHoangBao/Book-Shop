using back_end.DTOs.Auths.Responses;
using back_end.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleSetting _googleSetting;
        public GoogleAuthService(IOptions<GoogleSetting> options)
        {
            _googleSetting = options.Value;
        }
        public async Task<GoogleUserInfoResponse?> VerifyTokenAsync(string idToken)
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
