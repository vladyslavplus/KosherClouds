using KosherClouds.OrderService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Data;
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId);

                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.UnitPriceSnapshot).HasPrecision(18, 2);

                entity.Ignore(oi => oi.LineTotal);
            });
        }
    }