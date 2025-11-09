using KosherClouds.ProductService.Entities;
using KosherClouds.ServiceDefaults.Extensions;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ProductService.Data
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Enum>().HaveConversion<string>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.OwnsOne(p => p.HookahDetails, details =>
                {
                    details.Property(h => h.AdditionalParams).HasJsonConversion();

                    details.Property(h => h.TobaccoFlavor).IsRequired(false);
                    details.Property(h => h.BowlType).IsRequired(false);
                });

                entity.Property(p => p.Ingredients).HasJsonConversion();
                entity.Property(p => p.Allergens).HasJsonConversion();
                entity.Property(p => p.Photos).HasJsonConversion();
            });
        }
    }
}