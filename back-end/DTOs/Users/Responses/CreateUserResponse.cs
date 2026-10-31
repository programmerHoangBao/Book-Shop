using back_end.Enums;

namespace back_end.DTOs.Users.Responses
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public RoleUser Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool Enabled { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid StoreId { get; set; }
    }
}
