using KosherClouds.OrderService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KosherClouds.OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var orderStatusConverter = new EnumToStringConverter<OrderStatus>();
            var paymentTypeConverter = new EnumToStringConverter<PaymentType>();

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId);

                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                entity.Property(o => o.Status)
                      .HasMaxLength(50)
                      .HasConversion(orderStatusConverter);

                entity.Property(o => o.PaymentType)
                      .HasMaxLength(50)
                      .HasConversion(paymentTypeConverter);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.UnitPriceSnapshot).HasPrecision(18, 2);
                entity.Ignore(oi => oi.LineTotal);
            });
        }
    }
}