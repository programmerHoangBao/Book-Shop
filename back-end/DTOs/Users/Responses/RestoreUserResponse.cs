using back_end.DTOs.Users.Infos;

namespace back_end.DTOs.Users.Responses
{
    public class RestoreUserResponse
    {
        public List<Guid> ListUserIdSuccess { get; set; } = new();
        public List<ErrorDeleteUserInfo> ListErrors { get; set; } = new();
    }
}
