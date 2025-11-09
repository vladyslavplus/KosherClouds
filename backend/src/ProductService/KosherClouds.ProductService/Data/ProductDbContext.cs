namespace KosherClouds.ProductService.Data;

using Microsoft.EntityFrameworkCore;
using KosherClouds.ProductService.Entities;
using System.Text.Json;

    public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Category)
                      .HasConversion<string>(); 

                entity.OwnsOne(p => p.HookahDetails, details =>
                {
                    details.Property(h => h.Strength)
                           .HasConversion<string>(); 
                    
                    details.Property(h => h.AdditionalParams)
                           .HasConversion(
                               v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                               v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                           );
                    
                    details.Property(h => h.TobaccoFlavor).IsRequired(false);
                    details.Property(h => h.BowlType).IsRequired(false);
                });
                entity.Property(p => p.Ingredients)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      );
                
                entity.Property(p => p.Allergens)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      );
                      
                entity.Property(p => p.Photos)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      );
            });
        }
    }