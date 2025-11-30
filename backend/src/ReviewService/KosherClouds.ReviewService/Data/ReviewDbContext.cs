using KosherClouds.ReviewService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ReviewService.Data
{
    public class ReviewDbContext(DbContextOptions<ReviewDbContext> options)
        : DbContext(options)
    {
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => r.OrderId);
                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.Status);
                entity.HasIndex(r => r.ReviewType);
                entity.HasIndex(r => r.CreatedAt);

                entity.HasIndex(r => new { r.OrderId, r.UserId })
                    .IsUnique()
                    .HasFilter("\"ProductId\" IS NULL")
                    .HasDatabaseName("IX_Review_Order_User_Unique");

                entity.HasIndex(r => new { r.OrderId, r.ProductId, r.UserId })
                    .IsUnique()
                    .HasFilter("\"ProductId\" IS NOT NULL")
                    .HasDatabaseName("IX_Review_Order_Product_User_Unique");

                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(r => r.ReviewType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });
        }
    }
}