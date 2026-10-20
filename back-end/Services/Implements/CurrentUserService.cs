using back_end.Enums;
using System.Security.Claims;

namespace back_end.Services.Implements
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor, 
            ILogger<CurrentUserService> logger
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid? GetCurrentUserId()
        {
            string? userId = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out Guid id)
                ? id
                : null;
        }

        public RoleUser? GetRoleCurrentUser()
        {
            string? role = _httpContextAccessor
                        .HttpContext?
                        .User
                        .FindFirstValue(ClaimTypes.Role);

            if (Enum.TryParse<RoleUser>(
                    role,
                    ignoreCase: true,
                    out var roleUser)
                && Enum.IsDefined(typeof(RoleUser), roleUser))
            {
                return roleUser;
            }

            _logger.LogWarning(
                "Current user's role is missing or invalid.");
            return null;
        }
    }
}
