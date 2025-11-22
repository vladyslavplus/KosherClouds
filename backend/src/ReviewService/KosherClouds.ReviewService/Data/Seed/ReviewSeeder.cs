using KosherClouds.Common.Seed;
using KosherClouds.ReviewService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ReviewService.Data.Seed
{
    public static class ReviewSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();

            await context.Database.MigrateAsync();

            if (await context.Reviews.AnyAsync())
            {
                return;
            }

            var createdAt = DateTimeOffset.UtcNow.AddDays(-3);

            var review1 = new Review
            {
                Id = SharedSeedData.Review1Id,
                OrderId = SharedSeedData.Order1Id,
                ProductId = SharedSeedData.ProductKugelId,
                UserId = SharedSeedData.UserId,
                Rating = 5,
                Comment = "Amazing dish! Best Kugel I've ever tasted. Will definitely order again!",
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = createdAt,
                UpdatedAt = null
            };

            var review2 = new Review
            {
                Id = SharedSeedData.Review2Id,
                OrderId = SharedSeedData.Order1Id,
                ProductId = SharedSeedData.ProductFalafelSetId,
                UserId = SharedSeedData.UserId,
                Rating = 4,
                Comment = "Great falafel set, very fresh and flavorful. Just wish the portion was a bit larger.",
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = createdAt.AddHours(2),
                UpdatedAt = null
            };

            var review3 = new Review
            {
                Id = SharedSeedData.Review3Id,
                OrderId = SharedSeedData.Order2Id,
                ProductId = SharedSeedData.ProductHookahTropicalId,
                UserId = SharedSeedData.ManagerId,
                Rating = 5,
                Comment = "Excellent hookah experience! Tropical flavors are perfectly balanced and smooth.",
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = createdAt.AddDays(1),
                UpdatedAt = null
            };

            await context.Reviews.AddRangeAsync(review1, review2, review3);
            await context.SaveChangesAsync();
        }
    }
}