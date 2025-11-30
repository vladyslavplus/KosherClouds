using KosherClouds.Common.Seed;
using KosherClouds.Contracts.Reviews;
using KosherClouds.ReviewService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ReviewService.Data.Seed
{
    public static class ReviewSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReviewDbContext>>();

            await context.Database.MigrateAsync();

            if (await context.Reviews.AnyAsync())
            {
                logger.LogInformation("ReviewSeeder: Reviews already exist. Skipping seed.");
                return;
            }

            logger.LogInformation("ReviewSeeder: Waiting 8 seconds for ProductService to complete seeding...");
            await Task.Delay(TimeSpan.FromSeconds(8));

            var baseDate = DateTimeOffset.UtcNow.AddDays(-30);

            var reviews = new List<Review>
            {
                // ============================================
                // ORDER 1 - User: 1 Order Review + 12 Product Reviews
                // ============================================
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = null,
                    ReviewType = ReviewType.Order,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Amazing experience! Everything was perfect - food quality, service, and atmosphere. Will definitely order again!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(1),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Amazing dish! Best Kugel I've ever tasted.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(2),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 4,
                    Comment = "Great falafel set, very fresh and flavorful.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(3),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Love the mango and passion fruit combination!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(4),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Creamy and perfectly seasoned. Best hummus!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(5),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Perfect for breakfast! Eggs were cooked just right.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(6),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Soft, sweet, and perfectly braided!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(7),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductMatzoSoupId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Comforting and delicious! Matzo balls were fluffy.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(8),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Crispy, juicy, and delicious!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(9),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductBabkaId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Rich, chocolatey, and absolutely delicious!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(10),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Sweet berry flavor is amazing!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(11),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductLemonadeId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 4,
                    Comment = "Refreshing and not too sweet!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(12),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductMintTeaId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Authentic Moroccan taste!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(13),
                    UpdatedAt = null
                },

                // ============================================
                // ORDER 2 - Manager: 0 Order Reviews + 10 Product Reviews
                // ============================================
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Perfectly cooked, traditional taste.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(5),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Best falafel in town!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(7),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Excellent hookah experience!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(8),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductBrisketId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Melt-in-your-mouth tender!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(9),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Authentic taste, great texture.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(11),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Spicy and flavorful!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(12),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Traditional and delicious.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(13),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 4,
                    Comment = "Very good schnitzel!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(14),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 4,
                    Comment = "Good flavor combination.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(15),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductPomegranatJuiceId,
                    ReviewType = ReviewType.Product,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Fresh and delicious!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(16),
                    UpdatedAt = null
                },

                // ============================================
                // ORDER 3 - Admin: 1 Order Review + 0 Product Reviews
                // ============================================
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = null,
                    ReviewType = ReviewType.Order,
                    UserId = SharedSeedData.AdminId,
                    Rating = 4,
                    Comment = "Good overall experience. Food was tasty and delivery was on time.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(10),
                    UpdatedAt = null
                }
            };

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();

            logger.LogInformation("ReviewSeeder: Successfully seeded {Count} reviews", reviews.Count);

            logger.LogInformation("ReviewSeeder: Publishing ReviewCreatedEvent for Product reviews...");

            var publishCount = 0;
            foreach (var review in reviews)
            {
                if (review.ReviewType == ReviewType.Product && review.ProductId.HasValue)
                {
                    await publishEndpoint.Publish(new ReviewCreatedEvent
                    {
                        ReviewId = review.Id,
                        ProductId = review.ProductId.Value,
                        UserId = review.UserId,
                        Rating = review.Rating,
                        CreatedAt = review.CreatedAt
                    });

                    publishCount++;

                    logger.LogInformation(
                        "ReviewSeeder: Published ReviewCreatedEvent for Review {ReviewId}, Product {ProductId}",
                        review.Id, review.ProductId);
                }
            }

            logger.LogInformation("ReviewSeeder: Published {Count} ReviewCreatedEvents successfully", publishCount);
        }
    }
}