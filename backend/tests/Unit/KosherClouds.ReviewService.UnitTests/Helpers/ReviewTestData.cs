using Bogus;
using KosherClouds.Common.Seed;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Entities;
using KosherClouds.ReviewService.Parameters;

namespace KosherClouds.ReviewService.UnitTests.Helpers
{
    public static class ReviewTestData
    {
        private static readonly Faker _faker = new Faker();

        public static Review CreateValidReview()
        {
            return new Review
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Rating = _faker.Random.Int(1, 5),
                Comment = _faker.Lorem.Paragraph(),
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };
        }

        public static List<Review> CreateReviewList(int count)
        {
            var reviews = new List<Review>();
            for (int i = 0; i < count; i++)
            {
                reviews.Add(CreateValidReview());
            }
            return reviews;
        }

        public static Review CreateReviewForProduct(Guid productId, Guid userId, int rating = 5)
        {
            var review = CreateValidReview();
            review.ProductId = productId;
            review.UserId = userId;
            review.Rating = rating;
            return review;
        }

        public static Review CreateReviewForOrder(Guid orderId, Guid userId, Guid productId)
        {
            var review = CreateValidReview();
            review.OrderId = orderId;
            review.UserId = userId;
            review.ProductId = productId;
            return review;
        }

        public static Review CreateHiddenReview()
        {
            var review = CreateValidReview();
            review.Status = ReviewStatus.Hidden;
            review.ModeratedBy = Guid.NewGuid();
            review.ModeratedAt = DateTimeOffset.UtcNow;
            review.ModerationNotes = "Inappropriate content";
            return review;
        }

        public static Review CreateDeletedReview()
        {
            var review = CreateValidReview();
            review.Status = ReviewStatus.Deleted;
            review.UpdatedAt = DateTimeOffset.UtcNow;
            return review;
        }

        public static Review CreateOldReview(int daysOld)
        {
            var review = CreateValidReview();
            review.CreatedAt = DateTimeOffset.UtcNow.AddDays(-daysOld);
            return review;
        }

        public static ReviewCreateDto CreateValidReviewCreateDto(Guid? orderId = null, Guid? productId = null)
        {
            return new ReviewCreateDto
            {
                ProductId = productId ?? Guid.NewGuid(),
                OrderId = orderId ?? Guid.NewGuid(),
                Rating = _faker.Random.Int(1, 5),
                Comment = _faker.Lorem.Paragraph()
            };
        }

        public static ReviewUpdateDto CreateValidReviewUpdateDto()
        {
            return new ReviewUpdateDto
            {
                Rating = _faker.Random.Int(1, 5),
                Comment = _faker.Lorem.Paragraph()
            };
        }

        public static ReviewModerationDto CreateModerationDto(string action)
        {
            return new ReviewModerationDto
            {
                Action = action,
                ModerationNotes = "Moderation notes"
            };
        }

        public static ReviewParameters CreateReviewParameters()
        {
            return new ReviewParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
        }

        public static OrderDto CreateValidOrder(Guid userId, int daysAgo = 5)
        {
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            return new OrderDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = "Paid",
                TotalAmount = 250.00m,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-daysAgo),
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId1,
                        ProductNameSnapshot = "Test Product 1",
                        Quantity = 2,
                        UnitPriceSnapshot = 50.00m
                    },
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId2,
                        ProductNameSnapshot = "Test Product 2",
                        Quantity = 1,
                        UnitPriceSnapshot = 150.00m
                    }
                }
            };
        }

        public static OrderDto CreateExpiredOrder(Guid userId)
        {
            var order = CreateValidOrder(userId, 20);
            return order;
        }

        public static OrderDto CreatePendingOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = "Pending";
            return order;
        }

        public static List<OrderDto> CreateOrderList(Guid userId, int count)
        {
            var orders = new List<OrderDto>();
            for (int i = 0; i < count; i++)
            {
                orders.Add(CreateValidOrder(userId, i + 1));
            }
            return orders;
        }

        public static UserDto CreateUserDto()
        {
            return new UserDto
            {
                Id = Guid.NewGuid(),
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static UserDto CreateUserDtoWithId(Guid userId)
        {
            var user = CreateUserDto();
            user.Id = userId;
            return user;
        }

        public static Dictionary<Guid, string> CreateUserNamesDict(params Guid[] userIds)
        {
            var dict = new Dictionary<Guid, string>();
            foreach (var userId in userIds)
            {
                dict[userId] = _faker.Name.FullName();
            }
            return dict;
        }
    }
}