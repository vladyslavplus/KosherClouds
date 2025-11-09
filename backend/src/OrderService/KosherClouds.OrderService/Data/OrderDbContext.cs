using KosherClouds.OrderService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Data;
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                
                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId);
                      
                entity.HasMany(o => o.Payments)
                      .WithOne(pr => pr.Order)
                      .HasForeignKey(pr => pr.OrderId);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                
                entity.Property(oi => oi.UnitPriceSnapshot).HasPrecision(18, 2);
                
                entity.Ignore(oi => oi.LineTotal);
            });

            modelBuilder.Entity<PaymentRecord>(entity =>
            {
                entity.HasKey(pr => pr.Id);
                
                entity.Property(pr => pr.Amount).HasPrecision(18, 2);
            });
            
        }
    }