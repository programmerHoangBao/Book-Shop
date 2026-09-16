using back_end.Entities;
using back_end.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<AddressEntity> Addresses { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<BookImageEntity> BookImages { get; set; }
        public DbSet<BookCategoryEntity> BookCategories { get; set; }
        public DbSet<StoreEntity> Stores { get; set; }
        public DbSet<StoreStaffEntity> StoreStaffs { get; set; }
        public DbSet<InventoryEntity> Inventories { get; set; }
        public DbSet<InventoryTransactionEntity> InventoryTransactions { get; set; }
        public DbSet<CartEntity> Carts { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }
        public DbSet<PromotionEntity> Promotions { get; set; }
        public DbSet<PromotionBookEntity> PromotionBooks { get; set; }
        public DbSet<PromotionCatatogyEntity> PromotionCatatories { get; set; }
        public DbSet<PromotionUsageEntity> PromotionUsages { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<ReviewImageEntity> ReviewImages { get; set; }
        public DbSet<ReturnRequestEntity> ReturnRequests { get; set; }
        public DbSet<ReturnRequestEvidenceEntity> ReturnEvidenceEntities { get; set; }
        public DbSet<ReturnItemEntity> ReturnItems { get; set; }
        public DbSet<RefundEntity> Refunds { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add your entity configurations here

            // User
            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Address
            modelBuilder.Entity<AddressEntity>()
                .HasOne(ad => ad.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(ad => ad.UserId);

            modelBuilder.Entity<AddressEntity>()
                .HasIndex(ad => ad.UserId);

            // Category
            modelBuilder.Entity<CategoryEntity>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryEntity>()
                .HasIndex(c => c.ParentId);

            // Book image
            modelBuilder.Entity<BookImageEntity>()
                .HasOne(bi => bi.Book)
                .WithMany(b => b.Images)
                .HasForeignKey(bi => bi.BookId);

            modelBuilder.Entity<BookImageEntity>()
                .HasIndex(bi => bi.BookId);

            // Book category
            modelBuilder.Entity<BookCategoryEntity>()
                .HasKey(bc => new
                {
                    bc.BookId,
                    bc.CategoryId
                });

            modelBuilder.Entity<BookCategoryEntity>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<BookCategoryEntity>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);

            modelBuilder.Entity<BookCategoryEntity>()
                .HasIndex(bc => bc.BookId);

            modelBuilder.Entity<BookCategoryEntity>()
                .HasIndex(bc => bc.CategoryId);

            //Store staff
            modelBuilder.Entity<StoreStaffEntity>()
                .HasKey(ss => new
                {
                    ss.StoreId,
                    ss.UserId
                });

            modelBuilder.Entity<StoreStaffEntity>()
                .HasIndex(ss => ss.UserId)
                .IsUnique();

            modelBuilder.Entity<StoreStaffEntity>()
                .HasOne(ss => ss.User)
                .WithOne(u => u.StoreStaff)
                .HasForeignKey<StoreStaffEntity>(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreStaffEntity>()
                .HasOne(ss => ss.Store)
                .WithMany(s => s.StoreStaffs)
                .HasForeignKey(ss => ss.StoreId);

            modelBuilder.Entity<StoreStaffEntity>()
                .HasIndex(ss => ss.UserId);

            modelBuilder.Entity<StoreStaffEntity>()
                .HasIndex(ss => ss.StoreId);

            // Inventory
            modelBuilder.Entity<InventoryEntity>()
                .HasOne(i => i.Store)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.StoreId);
            modelBuilder.Entity<InventoryEntity>()
                .HasOne(i => i.Book)
                .WithMany(b => b.Inventories)
                .HasForeignKey(i => i.BookId);

            modelBuilder.Entity<InventoryEntity>()
                .HasIndex(i => i.StoreId);

            modelBuilder.Entity<InventoryEntity>()
                .HasIndex(i => i.BookId);

            // Inventory transaction
            modelBuilder.Entity<InventoryTransactionEntity>()
                .HasOne(it => it.Inventory)
                .WithMany(i => i.transactions)
                .HasForeignKey(it => it.InventoryId);

            modelBuilder.Entity<InventoryTransactionEntity>()
                .HasOne(it => it.User)
                .WithMany(u => u.InventoryTransactions)
                .HasForeignKey(it => it.UserId);

            modelBuilder.Entity<InventoryTransactionEntity>()
                .HasIndex(it => it.UserId);
            modelBuilder.Entity<InventoryTransactionEntity>()
                .HasIndex(it => it.InventoryId);

            // Cart
            modelBuilder.Entity<CartEntity>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            modelBuilder.Entity<CartEntity>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<CartEntity>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Cart item
            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Book)
                .WithMany(b => b.CartItems)
                .HasForeignKey(ci => ci.BookId);

            modelBuilder.Entity<CartItemEntity>()
                .HasIndex(ci => ci.CartId);
            modelBuilder.Entity<CartItemEntity>()
                .HasIndex(ci => ci.BookId);

            // Promotion 
            modelBuilder.Entity<PromotionEntity>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Promotion book
            modelBuilder.Entity<PromotionBookEntity>()
                .HasKey(pb => new
                {
                    pb.PromotionId,
                    pb.BookId
                });
            modelBuilder.Entity<PromotionBookEntity>()
                .HasOne(pb => pb.Book)
                .WithMany(b => b.PromotionBooks)
                .HasForeignKey(pb => pb.BookId);
            modelBuilder.Entity<PromotionBookEntity>()
                .HasOne(pb => pb.Promotion)
                .WithMany(p => p.PromotionBooks)
                .HasForeignKey(pb => pb.PromotionId);
            modelBuilder.Entity<PromotionBookEntity>()
                .HasIndex(pb => pb.BookId);
            modelBuilder.Entity<PromotionBookEntity>()
                .HasIndex(pb => pb.PromotionId);

            //Promotion catatory
            modelBuilder.Entity<PromotionCatatogyEntity>()
                .HasKey(pc => new
                {
                    pc.PromotionId,
                    pc.CategoryId
                });
            modelBuilder.Entity<PromotionCatatogyEntity>()
                .HasOne(pc => pc.Promotion)
                .WithMany(p => p.PromotionCatatogies)
                .HasForeignKey(pc => pc.PromotionId);
            modelBuilder.Entity<PromotionCatatogyEntity>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.PromotionCategories)
                .HasForeignKey(pc => pc.CategoryId);
            modelBuilder.Entity<PromotionCatatogyEntity>()
                .HasIndex(pc => pc.CategoryId);
            modelBuilder.Entity<PromotionCatatogyEntity>()
                .HasIndex(pc => pc.PromotionId);

            //Promotion Usage
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasOne(pu => pu.Promotion)
                .WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId);
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.PromotionUsages)
                .HasForeignKey(pu => pu.UserId);
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasOne(pu => pu.Order)
                .WithMany(o => o.PromotionUsages)
                .HasForeignKey(pu => pu.OrderId);
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasIndex(pu => pu.PromotionId);
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasIndex(pu => pu.UserId);
            modelBuilder.Entity<PromotionUsageEntity>()
                .HasIndex(pu => pu.OrderId);

            // Order
            modelBuilder.Entity<OrderEntity>()
                .HasIndex(o => o.OrderCode)
                .IsUnique(true);
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.Address)
                .WithMany(ad => ad.Orders)
                .HasForeignKey(o => o.AddressId);
            modelBuilder.Entity<OrderEntity>()
                .HasIndex(o => o.UserId);

            // Order Item
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Book)
                .WithMany(b => b.OrderItems)
                .HasForeignKey(oi => oi.BookId);
            modelBuilder.Entity<OrderItemEntity>()
                .HasIndex(oi => oi.OrderId);
            modelBuilder.Entity<OrderItemEntity>()
                .HasIndex(oi => oi.BookId);

            // Review
            modelBuilder.Entity<ReviewEntity>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<ReviewEntity>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId);
            modelBuilder.Entity<ReviewEntity>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId);
            modelBuilder.Entity<ReviewEntity>()
                .HasIndex(r => r.UserId);
            modelBuilder.Entity<ReviewEntity>()
                .HasIndex(r => r.BookId);
            modelBuilder.Entity<ReviewEntity>()
                .HasIndex(r => r.OrderId);

            // Review image
            modelBuilder.Entity<ReviewImageEntity>()
                .HasOne(ri => ri.Review)
                .WithMany(r => r.Images)
                .HasForeignKey(ri => ri.ReviewId);
            modelBuilder.Entity<ReviewImageEntity>()
                .HasIndex(ri => ri.ReviewId);

            // Return request
            modelBuilder.Entity<ReturnRequestEntity>()
                .HasIndex(rr => rr.OrderId)
                .IsUnique();

            modelBuilder.Entity<ReturnRequestEntity>()
                .HasOne(rr => rr.Order)
                .WithOne(o => o.ReturnRequest)
                .HasForeignKey<ReturnRequestEntity>(r => r.OrderId);
            modelBuilder.Entity<ReturnRequestEntity>()
                .HasOne(rr => rr.User)
                .WithMany(u => u.ReturnRequests)
                .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<ReturnRequestEntity>()
                .HasIndex(rr => rr.UserId);

            // Return request evidence
            modelBuilder.Entity<ReturnRequestEvidenceEntity>()
                .HasOne(rre => rre.ReturnRequest)
                .WithMany(rr => rr.Evidences)
                .HasForeignKey(rre => rre.ReturnRequestId);
            modelBuilder.Entity<ReturnRequestEvidenceEntity>()
                .HasIndex(rre => rre.ReturnRequestId);

            // Return item
            modelBuilder.Entity<ReturnItemEntity>()
                .HasIndex(ri => ri.OrderItemId)
                .IsUnique();
            modelBuilder.Entity<ReturnItemEntity>()
                .HasOne(ri => ri.ReturnRequest)
                .WithMany(rr => rr.Items)
                .HasForeignKey(ri => ri.ReturnRequestId);
            modelBuilder.Entity<ReturnItemEntity>()
                .HasOne(ri => ri.OrderItem)
                .WithOne(oi => oi.ReturnItem)
                .HasForeignKey<ReturnItemEntity>(ri => ri.OrderItemId);
            modelBuilder.Entity<ReturnItemEntity>()
                .HasIndex(ri => ri.ReturnRequestId);

            // Refund
            modelBuilder.Entity<RefundEntity>()
                .HasIndex(r => r.ReturnRequestId)
                .IsUnique();
            modelBuilder.Entity<RefundEntity>()
                .HasOne(r => r.ReturnRequest)
                .WithOne(rr => rr.RefundEntity)
                .HasForeignKey<RefundEntity>(r => r.ReturnRequestId);

            // Notification
            modelBuilder.Entity<NotificationEntity>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);
            modelBuilder.Entity<NotificationEntity>()
                .HasIndex(n => n.UserId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries<IAuditable>()
                .Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
