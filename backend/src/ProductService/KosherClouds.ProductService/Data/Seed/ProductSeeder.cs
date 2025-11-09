using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.Common.Seed;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ProductService.Data.Seed;
public static class ProductSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("ProductSeeder");

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Products.AnyAsync())
        {
            logger.LogInformation("ProductSeeder: Products table already contains data. Seeding skipped.");
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Id = SharedSeedData.ProductKugelId,
                Name = "Potato Kugel",
                Description = "Traditional Jewish baked potato pudding made with onions and eggs.",
                Price = 115.00m,
                Category = ProductCategory.Dish,
                SubCategory = "Classic",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.8,
                RatingCount = 95,
                Ingredients = new List<string> { "Potato", "Eggs", "Onion", "Oil" },
                Allergens = new List<string> { "Eggs" },
                Photos = new List<string> { "https://example.com/images/potato_kugel.jpg" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SharedSeedData.ProductFalafelSetId,
                Name = "Falafel Combo Set",
                Description = "Crispy falafel served with hummus, pita bread, and tahini sauce.",
                Price = 240.00m,
                Category = ProductCategory.Set,
                SubCategory = "Vegetarian",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.6,
                RatingCount = 150,
                Ingredients = new List<string> { "Chickpeas", "Parsley", "Tahini" },
                Allergens = new List<string> { "Sesame" },
                Photos = new List<string> { "https://example.com/images/falafel_combo.jpg" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SharedSeedData.ProductHookahTropicalId,
                Name = "Tropical Mix Hookah",
                Description = "A rich tropical blend with mango, passion fruit, and coconut flavor.",
                Price = 380.00m,
                Category = ProductCategory.Hookah,
                SubCategory = "Fruit",
                IsAvailable = true,
                Rating = 4.9,
                RatingCount = 85,
                Ingredients = new List<string> { "Tobacco", "Coconut coal" },
                Allergens = new List<string>(),
                Photos = new List<string> { "https://example.com/images/hookah_tropical.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Mango and Passion Fruit",
                    Strength = HookahStrength.Medium,
                    BowlType = "Clay",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Base", "Water" },
                        { "Add-on", "Orange slice" }
                    }
                },
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("ProductSeeder: Initial product data seeded successfully.");
    }
}