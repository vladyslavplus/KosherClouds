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
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Amazing dish! Best Kugel I've ever tasted. Will definitely order again!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(2),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Perfectly cooked, traditional taste. Reminded me of my grandmother's recipe.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(5),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 4,
                    Comment = "Very good, but could use a bit more seasoning for my taste.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(8),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    UserId = SharedSeedData.UserId,
                    Rating = 4,
                    Comment = "Great falafel set, very fresh and flavorful. Just wish the portion was a bit larger.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(3),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Best falafel in town! Crispy outside, soft inside. The tahini sauce is perfect!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(7),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 5,
                    Comment = "Excellent vegetarian option. Very filling and delicious!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(12),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Excellent hookah experience! Tropical flavors are perfectly balanced and smooth.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(4),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Love the mango and passion fruit combination. Very refreshing!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(9),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 4,
                    Comment = "Great flavor, though I prefer stronger tobacco. Still very good!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(15),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Creamy and perfectly seasoned. Best hummus I've had!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(1),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Authentic taste, great texture. Perfect with pita bread!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(6),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Perfect for breakfast! Eggs were cooked just right, sauce was amazing.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(3),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Spicy and flavorful! This is now my favorite breakfast dish.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(10),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 4,
                    Comment = "Very good, but I'd prefer it a bit less spicy. Still delicious!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(14),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Soft, sweet, and perfectly braided. Perfect for Shabbat dinner!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(4),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Traditional and delicious. Stays fresh for days!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(11),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductMatzoSoupId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Comforting and delicious! The matzo balls were so fluffy.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(2),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductMatzoSoupId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 4,
                    Comment = "Very good soup, reminds me of home. Could use more vegetables though.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(13),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductBrisketId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Melt-in-your-mouth tender! Best brisket I've ever had. Worth every penny!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(5),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductBrisketId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 5,
                    Comment = "Incredible! Slow-cooked to perfection with amazing flavor.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(16),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Crispy, juicy, and delicious! Served perfectly with lemon.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(7),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 4,
                    Comment = "Very good schnitzel! Breading was perfect, chicken was tender.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(12),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductBabkaId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Rich, chocolatey, and absolutely delicious! Best dessert on the menu!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(8),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductBabkaId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 5,
                    Comment = "Perfect with coffee! The chocolate swirl is amazing.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(17),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Sweet berry flavor is amazing! Very smooth and enjoyable.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(6),
                    UpdatedAt = null
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 4,
                    Comment = "Good flavor combination. A bit light for my preference but still good!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(11),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order3Id,
                    ProductId = SharedSeedData.ProductHookahMintId,
                    UserId = SharedSeedData.AdminId,
                    Rating = 5,
                    Comment = "Classic mint flavor done perfectly! Strong and refreshing.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(9),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductLemonadeId,
                    UserId = SharedSeedData.UserId,
                    Rating = 4,
                    Comment = "Refreshing and not too sweet. Perfect summer drink!",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(5),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order2Id,
                    ProductId = SharedSeedData.ProductPomegranatJuiceId,
                    UserId = SharedSeedData.ManagerId,
                    Rating = 5,
                    Comment = "Fresh and delicious! You can taste the quality.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(10),
                    UpdatedAt = null
                },

                new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = SharedSeedData.Order1Id,
                    ProductId = SharedSeedData.ProductMintTeaId,
                    UserId = SharedSeedData.UserId,
                    Rating = 5,
                    Comment = "Authentic Moroccan taste! The mint is so fresh and aromatic.",
                    IsVerifiedPurchase = true,
                    Status = ReviewStatus.Published,
                    CreatedAt = baseDate.AddDays(4),
                    UpdatedAt = null
                }
            };

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();

            logger.LogInformation("ReviewSeeder: Successfully seeded {Count} reviews", reviews.Count);

            logger.LogInformation("ReviewSeeder: Publishing ReviewCreatedEvent for each review...");

            foreach (var review in reviews)
            {
                await publishEndpoint.Publish(new ReviewCreatedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    CreatedAt = review.CreatedAt
                });

                logger.LogInformation(
                    "ReviewSeeder: Published ReviewCreatedEvent for Review {ReviewId}, Product {ProductId}",
                    review.Id, review.ProductId);
            }

            logger.LogInformation("ReviewSeeder: All ReviewCreatedEvents published successfully");
        }
    }
}