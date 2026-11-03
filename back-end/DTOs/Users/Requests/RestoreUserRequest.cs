namespace back_end.DTOs.Users.Requests
{
    public class RestoreUserRequest
    {
        public List<Guid> UserIds { get; set; } = new();
    }
}
