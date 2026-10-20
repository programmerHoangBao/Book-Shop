using back_end.Enums;

namespace back_end.Services
{
    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        RoleUser? GetRoleCurrentUser();
    }
}
