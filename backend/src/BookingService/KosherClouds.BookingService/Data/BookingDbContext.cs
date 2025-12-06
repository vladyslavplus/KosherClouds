using KosherClouds.BookingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<HookahBooking> Hookahs => Set<HookahBooking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasIndex(b => b.UserId)
                    .HasDatabaseName("IX_Bookings_UserId");

                entity.HasIndex(b => b.BookingDateTime)
                    .HasDatabaseName("IX_Bookings_BookingDateTime");

                entity.HasIndex(b => b.Status)
                    .HasDatabaseName("IX_Bookings_Status");

                entity.HasIndex(b => new { b.Zone, b.BookingDateTime, b.Status })
                    .HasDatabaseName("IX_Bookings_Zone_DateTime_Status");

                entity.HasIndex(b => new { b.UserId, b.Status })
                    .HasDatabaseName("IX_Bookings_UserId_Status");

                entity.Property(b => b.Zone)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(b => b.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired()
                    .HasDefaultValue(BookingStatus.Pending);

                entity.Property(b => b.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(b => b.Comment)
                    .HasMaxLength(1000);

                entity.Property(b => b.Adults)
                    .IsRequired();

                entity.Property(b => b.Children)
                    .IsRequired();

                entity.Property(b => b.UpdatedAt)
                    .IsRequired(false);
            });

            modelBuilder.Entity<HookahBooking>(entity =>
            {
                entity.HasKey(h => h.Id);

                entity.HasIndex(h => h.BookingId)
                    .HasDatabaseName("IX_Hookahs_BookingId");

                entity.HasIndex(h => h.ProductId)
                    .HasDatabaseName("IX_Hookahs_ProductId");

                entity.HasOne(h => h.Booking)
                    .WithMany(b => b.Hookahs)
                    .HasForeignKey(h => h.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(h => h.Strength)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(h => h.ProductId)
                    .IsRequired(false);

                entity.Property(h => h.ProductName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(h => h.ProductNameUk)
                    .HasMaxLength(100);

                entity.Property(h => h.TobaccoFlavor)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(h => h.TobaccoFlavorUk)
                    .HasMaxLength(100);

                entity.Property(h => h.Notes)
                    .HasMaxLength(500);

                entity.Property(h => h.ServeAfterMinutes)
                    .IsRequired(false);

                entity.Property(h => h.PriceSnapshot)
                    .HasPrecision(10, 2)
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}