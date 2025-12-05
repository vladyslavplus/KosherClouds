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
                ReviewType = ReviewType.Product,
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
            review.ReviewType = ReviewType.Product;
            review.UserId = userId;
            review.Rating = rating;
            return review;
        }

        public static Review CreateReviewForOrder(Guid orderId, Guid userId, Guid? productId = null)
        {
            var review = CreateValidReview();
            review.OrderId = orderId;
            review.UserId = userId;
            review.ProductId = productId;
            review.ReviewType = productId.HasValue ? ReviewType.Product : ReviewType.Order;
            return review;
        }

        public static Review CreateOrderReview(Guid orderId, Guid userId, int rating = 5)
        {
            return new Review
            {
                Id = Guid.NewGuid(),
                ProductId = null,
                ReviewType = ReviewType.Order,
                UserId = userId,
                OrderId = orderId,
                Rating = rating,
                Comment = _faker.Lorem.Paragraph(),
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };
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

        public static ReviewCreateDto CreateValidReviewCreateDto(Guid? orderId = null, Guid? productId = null, ReviewType? reviewType = null)
        {
            return new ReviewCreateDto
            {
                ProductId = productId ?? Guid.NewGuid(),
                OrderId = orderId ?? Guid.NewGuid(),
                ReviewType = reviewType ?? ReviewType.Product,
                Rating = _faker.Random.Int(1, 5),
                Comment = _faker.Lorem.Paragraph()
            };
        }

        public static ReviewCreateDto CreateOrderReviewDto(Guid orderId)
        {
            return new ReviewCreateDto
            {
                ProductId = null,
                OrderId = orderId,
                ReviewType = ReviewType.Order,
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
                        ProductNameSnapshotUk = "Тестовий Продукт 1",
                        Quantity = 2,
                        UnitPriceSnapshot = 50.00m
                    },
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId2,
                        ProductNameSnapshot = "Test Product 2",
                        ProductNameSnapshotUk = "Тестовий Продукт 2",
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

        public static Review CreateReviewWithAllFields()
        {
            return new Review
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ReviewType = ReviewType.Product,
                UserId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Excellent product!",
                IsVerifiedPurchase = true,
                Status = ReviewStatus.Published,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null,
                ModerationNotes = null,
                ModeratedBy = null,
                ModeratedAt = null
            };
        }

        public static ReviewCreateDto CreateOrderReviewDtoWithRating(Guid orderId, int rating)
        {
            return new ReviewCreateDto
            {
                OrderId = orderId,
                ReviewType = ReviewType.Order,
                ProductId = null,
                Rating = rating,
                Comment = $"Order review with rating {rating}"
            };
        }

        public static ReviewModerationDto CreateModerationDtoWithNotes(string action, string notes)
        {
            return new ReviewModerationDto
            {
                Action = action,
                ModerationNotes = notes
            };
        }

        public static List<Review> CreateReviewsWithDifferentStatuses()
        {
            return new List<Review>
            {
                CreateValidReview(),
                CreateHiddenReview(),
                CreateDeletedReview()
            };
        }

        public static Review CreateReviewWithRating(int rating)
        {
            var review = CreateValidReview();
            review.Rating = rating;
            return review;
        }

        public static OrderDto CreateOrderWithMultipleItems(Guid userId, int itemCount)
        {
            var order = CreateValidOrder(userId);
            order.Items = new List<OrderItemDto>();

            for (int i = 0; i < itemCount; i++)
            {
                order.Items.Add(new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductNameSnapshot = $"Product {i + 1}",
                    ProductNameSnapshotUk = $"Продукт {i + 1}",
                    Quantity = i + 1,
                    UnitPriceSnapshot = (i + 1) * 10.00m
                });
            }

            return order;
        }
    }
}