using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Entities;
using KosherClouds.ReviewService.IntegrationTests.Infrastructure;

namespace KosherClouds.ReviewService.IntegrationTests.Controllers
{
    public class ReviewsControllerTests : IClassFixture<ReviewServiceWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public ReviewsControllerTests(ReviewServiceWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            MockOrderApiClient.ClearOrders();
            MockUserApiClient.ClearUsers();
        }

        [Fact]
        public async Task GetReviews_WithoutAuthentication_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/reviews");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetReviews_ShouldReturnPagedResults()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            await CreateReview(orderId, productId, ReviewType.Product);

            var response = await _client.GetAsync("/api/reviews?pageSize=10");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviews = await response.Content.ReadFromJsonAsync<List<ReviewResponseDto>>(JsonOptions);
            reviews.Should().NotBeNull();
            response.Headers.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task GetReviews_FilterByProductId_ShouldReturnFilteredResults()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId1 = CreateMockOrderWithProduct(userId, out var productId1);
            var orderId2 = CreateMockOrderWithProduct(userId, out var productId2);

            await CreateReview(orderId1, productId1, ReviewType.Product);
            await CreateReview(orderId2, productId2, ReviewType.Product);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync($"/api/reviews?productId={productId1}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviews = await response.Content.ReadFromJsonAsync<List<ReviewResponseDto>>(JsonOptions);
            reviews.Should().NotBeNull();
            reviews!.Should().OnlyContain(r => r.ProductId == productId1);
        }

        [Fact]
        public async Task GetReviews_FilterByRatingRange_ShouldReturnFilteredResults()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId1 = CreateMockOrderWithProduct(userId, out var productId1);
            var orderId2 = CreateMockOrderWithProduct(userId, out var productId2);

            await CreateReview(orderId1, productId1, ReviewType.Product, rating: 2);
            await CreateReview(orderId2, productId2, ReviewType.Product, rating: 5);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/reviews?minRating=4&maxRating=5");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviews = await response.Content.ReadFromJsonAsync<List<ReviewResponseDto>>(JsonOptions);
            reviews.Should().NotBeNull();
            reviews!.Should().OnlyContain(r => r.Rating >= 4 && r.Rating <= 5);
        }

        [Fact]
        public async Task GetReviews_WithInvalidRatingRange_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/reviews?minRating=5&maxRating=2");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReviewById_WithValidId_ShouldReturnReview()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync($"/api/reviews/{reviewId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
            review.Should().NotBeNull();
            review!.Id.Should().Be(reviewId);
        }

        [Fact]
        public async Task GetReviewById_WithInvalidId_ShouldReturnNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/reviews/{nonExistentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMyOrdersToReview_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/reviews/my-orders-to-review");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyOrdersToReview_WithPaidOrders_ShouldReturnOrders()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            CreateMockOrderWithProduct(userId, out _);

            var response = await _client.GetAsync("/api/reviews/my-orders-to-review");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var orders = await response.Content.ReadFromJsonAsync<List<OrderToReviewDto>>(JsonOptions);
            orders.Should().NotBeNull();
            orders!.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task GetReviewableProducts_WithValidOrder_ShouldReturnProducts()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out _);

            var response = await _client.GetAsync($"/api/reviews/order/{orderId}/reviewable-products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<List<ReviewableProductDto>>(JsonOptions);
            products.Should().NotBeNull();
            products!.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task GetReviewableProducts_WithInvalidOrder_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var nonExistentOrderId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/reviews/order/{nonExistentOrderId}/reviewable-products");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReviewableProducts_ForDifferentUser_ShouldReturnUnauthorized()
        {
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var orderId = CreateMockOrderWithProduct(userId1, out _);

            _client.AddAuthorizationHeader(userId2);

            var response = await _client.GetAsync($"/api/reviews/order/{orderId}/reviewable-products");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReview_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var dto = new ReviewCreateDto
            {
                OrderId = Guid.NewGuid(),
                Rating = 5,
                ReviewType = ReviewType.Order
            };

            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReview_ForProduct_ShouldReturnCreated()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);

            var dto = new ReviewCreateDto
            {
                OrderId = orderId,
                ProductId = productId,
                Rating = 5,
                Comment = "Great product!",
                ReviewType = ReviewType.Product
            };

            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
            review.Should().NotBeNull();
            review!.Rating.Should().Be(5);
            review.ReviewType.Should().Be(ReviewType.Product);
        }

        [Fact]
        public async Task CreateReview_ForOrder_ShouldReturnCreated()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out _);

            var dto = new ReviewCreateDto
            {
                OrderId = orderId,
                Rating = 4,
                Comment = "Good service!",
                ReviewType = ReviewType.Order
            };

            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
            review.Should().NotBeNull();
            review!.ReviewType.Should().Be(ReviewType.Order);
        }

        [Fact]
        public async Task CreateReview_Duplicate_ShouldReturnConflict()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);

            var dto = new ReviewCreateDto
            {
                OrderId = orderId,
                ProductId = productId,
                Rating = 5,
                ReviewType = ReviewType.Product
            };

            await _client.PostAsJsonAsync("/api/reviews", dto);
            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateReview_ForNonExistentOrder_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var dto = new ReviewCreateDto
            {
                OrderId = Guid.NewGuid(),
                Rating = 5,
                ReviewType = ReviewType.Order
            };

            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateReview_AsOwner_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product, rating: 3);

            var updateDto = new ReviewUpdateDto
            {
                Rating = 5,
                Comment = "Updated comment"
            };

            var response = await _client.PutAsJsonAsync($"/api/reviews/{reviewId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
            review!.Rating.Should().Be(5);
            review.Comment.Should().Be("Updated comment");
        }

        [Fact]
        public async Task UpdateReview_ByDifferentUser_ShouldReturnUnauthorized()
        {
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId1);
            var orderId = CreateMockOrderWithProduct(userId1, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.AddAuthorizationHeader(userId2);
            var updateDto = new ReviewUpdateDto { Rating = 1 };

            var response = await _client.PutAsJsonAsync($"/api/reviews/{reviewId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SoftDeleteReview_AsOwner_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            var response = await _client.PostAsync($"/api/reviews/{reviewId}/soft-delete", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task SoftDeleteReview_AsAdmin_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            var response = await _client.PostAsync($"/api/reviews/{reviewId}/soft-delete", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteReview_WithoutAdminRole_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            var response = await _client.DeleteAsync($"/api/reviews/{reviewId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteReview_AsAdmin_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            var response = await _client.DeleteAsync($"/api/reviews/{reviewId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ModerateReview_WithoutAdminOrManager_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            var moderationDto = new ReviewModerationDto
            {
                Action = "Hide",
                ModerationNotes = "Inappropriate content"
            };

            var response = await _client.PatchAsync($"/api/reviews/{reviewId}/moderate",
                JsonContent.Create(moderationDto));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ModerateReview_AsAdmin_Hide_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            var moderationDto = new ReviewModerationDto
            {
                Action = "Hide",
                ModerationNotes = "Spam"
            };

            var response = await _client.PatchAsync($"/api/reviews/{reviewId}/moderate",
                JsonContent.Create(moderationDto));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ModerateReview_AsManager_Publish_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = CreateMockOrderWithProduct(userId, out var productId);
            var reviewId = await CreateReview(orderId, productId, ReviewType.Product);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            await _client.PatchAsync($"/api/reviews/{reviewId}/moderate",
                JsonContent.Create(new ReviewModerationDto { Action = "Hide", ModerationNotes = "Test" }));

            _client.AddAuthorizationHeader(managerId, new[] { "Manager" });
            var response = await _client.PatchAsync($"/api/reviews/{reviewId}/moderate",
                JsonContent.Create(new ReviewModerationDto { Action = "Publish", ModerationNotes = "Approved" }));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private static int _orderCounter = 0;

        private Guid CreateMockOrderWithProduct(Guid userId, out Guid productId)
        {
            var counter = Interlocked.Increment(ref _orderCounter);
            productId = Guid.NewGuid();

            var items = new List<OrderItemDto>
            {
                new()
                {
                    ProductId = productId,
                    ProductNameSnapshot = $"Test Product {counter}",
                    ProductNameSnapshotUk = $"Тестовий продукт {counter}",
                    UnitPriceSnapshot = 100m,
                    Quantity = 1
                }
            };

            MockUserApiClient.AddMockUser(userId, "Test", "User", $"testuser{counter}");
            return MockOrderApiClient.CreateMockOrder(userId, items);
        }

        private async Task<Guid> CreateReview(
            Guid orderId,
            Guid? productId,
            ReviewType reviewType,
            int rating = 5)
        {
            var dto = new ReviewCreateDto
            {
                OrderId = orderId,
                ProductId = productId,
                Rating = rating,
                Comment = "Test review",
                ReviewType = reviewType
            };

            var response = await _client.PostAsJsonAsync("/api/reviews", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"CreateReview failed. Status: {response.StatusCode}. Body: {errorContent}");
            }

            var review = await response.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
            return review!.Id;
        }
    }
}