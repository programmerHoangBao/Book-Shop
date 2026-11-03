namespace back_end.DTOs.Users.Requests
{
    public class DeleteUsersRequest
    {
        public List<Guid> UserIds { get; set; } = new();
    }
}
