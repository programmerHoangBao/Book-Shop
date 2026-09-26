using back_end.Entities;

namespace back_end.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(UserEntity user);
        string GenerateRefreshToken();
    }
}
