using back_end.Entities.Interfaces;

namespace back_end.Entities
{
    public class StoreStaffEntity : IAuditable
    {
        public Guid StoreId { get; set; } = Guid.CreateVersion7();
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public StoreEntity? Store { get; set; }
        public UserEntity? User { get; set; }
    }
}
