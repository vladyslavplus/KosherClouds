using AutoMapper;
using FluentAssertions;
using KosherClouds.Contracts.Reviews;
using KosherClouds.ReviewService.Data;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Entities;
using KosherClouds.ReviewService.Parameters;
using KosherClouds.ReviewService.Services.External;
using KosherClouds.ReviewService.UnitTests.Helpers;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewServiceClass = KosherClouds.ReviewService.Services.ReviewService;

namespace KosherClouds.ReviewService.UnitTests.Services
{
    public class ReviewServiceTests : IDisposable
    {
        private readonly ReviewDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<ISortHelperFactory> _sortHelperFactoryMock;
        private readonly Mock<ISortHelper<Review>> _sortHelperMock;
        private readonly Mock<ILogger<ReviewServiceClass>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IOrderApiClient> _orderApiClientMock;
        private readonly Mock<IUserApiClient> _userApiClientMock;
        private readonly ReviewServiceClass _reviewService;
        private bool _disposed;

        public ReviewServiceTests()
        {
            _dbContext = MockReviewDbContextFactory.Create();
            _mapper = AutoMapperFactory.Create();
            _loggerMock = new Mock<ILogger<ReviewServiceClass>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _orderApiClientMock = MockApiClientsFactory.CreateOrderApiClient();
            _userApiClientMock = MockApiClientsFactory.CreateUserApiClient();

            _sortHelperMock = new Mock<ISortHelper<Review>>();
            _sortHelperMock
                .Setup(x => x.ApplySort(It.IsAny<IQueryable<Review>>(), It.IsAny<string>()))
                .Returns<IQueryable<Review>, string>((query, orderBy) => query);

            _sortHelperFactoryMock = new Mock<ISortHelperFactory>();
            _sortHelperFactoryMock
                .Setup(x => x.Create<Review>())
                .Returns(_sortHelperMock.Object);

            _reviewService = new ReviewServiceClass(
                _dbContext,
                _mapper,
                _sortHelperFactoryMock.Object,
                _loggerMock.Object,
                _publishEndpointMock.Object,
                _orderApiClientMock.Object,
                _userApiClientMock.Object);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region GetReviewsAsync Tests

        [Fact]
        public async Task GetReviewsAsync_WithNoFilters_ReturnsOnlyPublishedReviews()
        {
            // Arrange
            var publishedReview = ReviewTestData.CreateValidReview();
            var hiddenReview = ReviewTestData.CreateHiddenReview();
            var deletedReview = ReviewTestData.CreateDeletedReview();

            await _dbContext.Reviews.AddRangeAsync(publishedReview, hiddenReview, deletedReview);
            await _dbContext.SaveChangesAsync();

            var parameters = ReviewTestData.CreateReviewParameters();

            _userApiClientMock.SetupGetUserNamesByIds(
                ReviewTestData.CreateUserNamesDict(publishedReview.UserId));

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Status.Should().Be(ReviewStatus.Published.ToString());
        }

        [Fact]
        public async Task GetReviewsAsync_WithProductIdFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var review1 = ReviewTestData.CreateReviewForProduct(productId, Guid.NewGuid());
            var review2 = ReviewTestData.CreateValidReview();

            await _dbContext.Reviews.AddRangeAsync(review1, review2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                ProductId = productId,
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task GetReviewsAsync_WithUserIdFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var review1 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), userId);
            var review2 = ReviewTestData.CreateValidReview();

            await _dbContext.Reviews.AddRangeAsync(review1, review2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetReviewsAsync_WithOrderIdFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var review1 = ReviewTestData.CreateReviewForOrder(orderId, Guid.NewGuid());
            var review2 = ReviewTestData.CreateReviewForOrder(Guid.NewGuid(), Guid.NewGuid());

            await _dbContext.Reviews.AddRangeAsync(review1, review2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                OrderId = orderId,
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].OrderId.Should().Be(orderId);
        }

        [Fact]
        public async Task GetReviewsAsync_WithRatingFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var review1 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), Guid.NewGuid(), 5);
            var review2 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), Guid.NewGuid(), 3);
            var review3 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), Guid.NewGuid(), 1);

            await _dbContext.Reviews.AddRangeAsync(review1, review2, review3);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                MinRating = 3,
                MaxRating = 5,
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(r => r.Rating.Should().BeInRange(3, 5));
        }

        [Fact]
        public async Task GetReviewsAsync_WithVerifiedOnlyFilter_ReturnsOnlyVerifiedReviews()
        {
            // Arrange
            var verifiedReview = ReviewTestData.CreateValidReview();
            verifiedReview.IsVerifiedPurchase = true;

            var unverifiedReview = ReviewTestData.CreateValidReview();
            unverifiedReview.IsVerifiedPurchase = false;

            await _dbContext.Reviews.AddRangeAsync(verifiedReview, unverifiedReview);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                VerifiedOnly = true,
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].IsVerifiedPurchase.Should().BeTrue();
        }

        [Fact]
        public async Task GetReviewsAsync_WithStatusFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var publishedReview = ReviewTestData.CreateValidReview();
            var hiddenReview = ReviewTestData.CreateHiddenReview();

            await _dbContext.Reviews.AddRangeAsync(publishedReview, hiddenReview);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                Status = ReviewStatus.Hidden.ToString(),
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Status.Should().Be(ReviewStatus.Hidden.ToString());
        }

        [Fact]
        public async Task GetReviewsAsync_WithReviewTypeFilter_ReturnsOnlyMatchingType()
        {
            // Arrange
            var productReview1 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), Guid.NewGuid());
            var productReview2 = ReviewTestData.CreateReviewForProduct(Guid.NewGuid(), Guid.NewGuid());
            var orderReview = ReviewTestData.CreateOrderReview(Guid.NewGuid(), Guid.NewGuid());

            await _dbContext.Reviews.AddRangeAsync(productReview1, productReview2, orderReview);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                ReviewType = ReviewType.Product.ToString(),
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(r => r.ReviewType.Should().Be(ReviewType.Product));
        }

        [Fact]
        public async Task GetReviewsAsync_WithDateRangeFilter_ReturnsMatchingReviews()
        {
            // Arrange
            var review1 = ReviewTestData.CreateOldReview(5);
            var review2 = ReviewTestData.CreateOldReview(10);
            var review3 = ReviewTestData.CreateOldReview(20);

            await _dbContext.Reviews.AddRangeAsync(review1, review2, review3);
            await _dbContext.SaveChangesAsync();

            var parameters = new ReviewParameters
            {
                MinCreatedDate = DateTimeOffset.UtcNow.AddDays(-12),
                MaxCreatedDate = DateTimeOffset.UtcNow.AddDays(-4),
                PageNumber = 1,
                PageSize = 10
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetReviewsAsync_PopulatesUserNames()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var parameters = ReviewTestData.CreateReviewParameters();

            _userApiClientMock.SetupGetUserNamesByIds(
                new Dictionary<Guid, string> { { review.UserId, "John Doe" } });

            // Act
            var result = await _reviewService.GetReviewsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result[0].UserName.Should().Be("John Doe");
        }

        #endregion

        #region GetReviewByIdAsync Tests

        [Fact]
        public async Task GetReviewByIdAsync_WithValidId_ReturnsReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var userNames = new Dictionary<Guid, string>
            {
                { review.UserId, "John Doe" }
            };
            _userApiClientMock.SetupGetUserNamesByIds(userNames);

            // Act
            var result = await _reviewService.GetReviewByIdAsync(review.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(review.Id);
            result.UserName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetReviewByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _reviewService.GetReviewByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetReviewByIdAsync_WithUserWithoutUserName_ReturnsReviewWithoutUserName()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // No user names in dictionary
            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.GetReviewByIdAsync(review.Id);

            // Assert
            result.Should().NotBeNull();
            result!.UserName.Should().BeNull();
        }

        #endregion

        #region GetOrdersToReviewAsync Tests

        [Fact]
        public async Task GetOrdersToReviewAsync_WithPaidOrders_ReturnsOrdersToReview()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orders = ReviewTestData.CreateOrderList(userId, 2);

            _orderApiClientMock.SetupGetPaidOrders(userId, orders);

            // Act
            var result = await _reviewService.GetOrdersToReviewAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Products.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetOrdersToReviewAsync_WithExistingReviews_MarksProductsAsReviewed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var productId = order.Items[0].ProductId;

            var existingReview = ReviewTestData.CreateReviewForOrder(order.Id, userId, productId);
            await _dbContext.Reviews.AddAsync(existingReview);
            await _dbContext.SaveChangesAsync();

            _orderApiClientMock.SetupGetPaidOrders(userId, new List<OrderDto> { order });

            // Act
            var result = await _reviewService.GetOrdersToReviewAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result[0].Products[0].AlreadyReviewed.Should().BeTrue();
            result[0].Products[0].ExistingReviewId.Should().Be(existingReview.Id);
            result[0].ReviewableProductsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetOrdersToReviewAsync_WithOrderReview_MarksOrderAsReviewed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);

            var existingOrderReview = ReviewTestData.CreateOrderReview(order.Id, userId);
            await _dbContext.Reviews.AddAsync(existingOrderReview);
            await _dbContext.SaveChangesAsync();

            _orderApiClientMock.SetupGetPaidOrders(userId, new List<OrderDto> { order });

            // Act
            var result = await _reviewService.GetOrdersToReviewAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result[0].OrderReviewExists.Should().BeTrue();
            result[0].OrderReviewId.Should().Be(existingOrderReview.Id);
        }

        [Fact]
        public async Task GetOrdersToReviewAsync_CalculatesDaysLeftCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId, daysAgo: 10);

            _orderApiClientMock.SetupGetPaidOrders(userId, new List<OrderDto> { order });

            // Act
            var result = await _reviewService.GetOrdersToReviewAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result[0].DaysLeftToReview.Should().Be(4); // 14 - 10 = 4
        }

        [Fact]
        public async Task GetOrdersToReviewAsync_WithNoOrders_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _orderApiClientMock.SetupGetPaidOrders(userId, new List<OrderDto>());

            // Act
            var result = await _reviewService.GetOrdersToReviewAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetReviewableProductsAsync Tests

        [Fact]
        public async Task GetReviewableProductsAsync_WithValidOrder_ReturnsProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            var result = await _reviewService.GetReviewableProductsAsync(order.Id, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.AlreadyReviewed.Should().BeFalse());
        }

        [Fact]
        public async Task GetReviewableProductsAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _orderApiClientMock.SetupGetOrderById(orderId, null);

            // Act
            Func<Task> act = async () => await _reviewService.GetReviewableProductsAsync(orderId, userId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task GetReviewableProductsAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.GetReviewableProductsAsync(order.Id, wrongUserId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You can only view your own orders");
        }

        [Fact]
        public async Task GetReviewableProductsAsync_WithPendingOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreatePendingOrder(userId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.GetReviewableProductsAsync(order.Id, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You can only review paid orders");
        }

        [Fact]
        public async Task GetReviewableProductsAsync_WithExpiredReviewPeriod_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateExpiredOrder(userId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.GetReviewableProductsAsync(order.Id, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review period has expired (14 days)");
        }

        #endregion

        #region CreateReviewAsync Tests

        [Fact]
        public async Task CreateReviewAsync_WithValidData_CreatesReview()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, order.Items[0].ProductId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);
            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(dto.ProductId);
            result.Rating.Should().Be(dto.Rating);
            result.IsVerifiedPurchase.Should().BeTrue();
            result.Status.Should().Be(ReviewStatus.Published.ToString());

            var savedReview = await _dbContext.Reviews.FindAsync(result.Id);
            savedReview.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateReviewAsync_WithOrderReview_CreatesSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = ReviewTestData.CreateOrderReviewDto(order.Id);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);
            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.ReviewType.Should().Be(ReviewType.Order);
            result.ProductId.Should().BeNull();
            result.Rating.Should().Be(dto.Rating);
            result.IsVerifiedPurchase.Should().BeTrue();
        }

        [Fact]
        public async Task CreateReviewAsync_PublishesReviewCreatedEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, order.Items[0].ProductId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);
            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewCreatedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task CreateReviewAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = ReviewTestData.CreateValidReviewCreateDto();

            _orderApiClientMock.SetupGetOrderById(dto.OrderId, null);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task CreateReviewAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, order.Items[0].ProductId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(wrongUserId, dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("This order doesn't belong to you");
        }

        [Fact]
        public async Task CreateReviewAsync_WithPendingOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreatePendingOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, order.Items[0].ProductId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You can only review paid orders");
        }

        [Fact]
        public async Task CreateReviewAsync_WithExpiredReviewPeriod_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateExpiredOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, order.Items[0].ProductId);

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review period has expired (14 days)");
        }

        [Fact]
        public async Task CreateReviewAsync_WithOrderReviewHavingProductId_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = new ReviewCreateDto
            {
                OrderId = order.Id,
                ReviewType = ReviewType.Order,
                ProductId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Test"
            };

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Order reviews cannot have ProductId");
        }

        [Fact]
        public async Task CreateReviewAsync_WithProductReviewMissingProductId_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = new ReviewCreateDto
            {
                OrderId = order.Id,
                ReviewType = ReviewType.Product,
                ProductId = null,
                Rating = 5,
                Comment = "Test"
            };

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("ProductId is required for product reviews");
        }

        [Fact]
        public async Task CreateReviewAsync_WithProductNotInOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, Guid.NewGuid());

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This product was not in your order");
        }

        [Fact]
        public async Task CreateReviewAsync_WithExistingReview_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = ReviewTestData.CreateValidOrder(userId);
            var productId = order.Items[0].ProductId;
            var dto = ReviewTestData.CreateValidReviewCreateDto(order.Id, productId);

            var existingReview = ReviewTestData.CreateReviewForOrder(order.Id, userId, productId);
            await _dbContext.Reviews.AddAsync(existingReview);
            await _dbContext.SaveChangesAsync();

            _orderApiClientMock.SetupGetOrderById(order.Id, order);

            // Act
            Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You already reviewed this product in this order");
        }

        #endregion

        #region UpdateReviewAsync Tests

        [Fact]
        public async Task UpdateReviewAsync_WithValidData_UpdatesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            var result = await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Rating.Should().Be(dto.Rating!.Value);
            result.Comment.Should().Be(dto.Comment);

            var updatedReview = await _dbContext.Reviews.FindAsync(review.Id);
            updatedReview!.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateReviewAsync_WithRatingChange_PublishesReviewUpdatedEvent()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            review.Rating = 3;
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = new ReviewUpdateDto { Rating = 5 };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewUpdatedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task UpdateReviewAsync_WithNoRatingChange_DoesNotPublishEvent()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            review.Rating = 5;
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = new ReviewUpdateDto
            {
                Rating = 5,
                Comment = "Updated comment"
            };

            _userApiClientMock.SetupGetUserNamesByIds(new Dictionary<Guid, string>());

            // Act
            await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewUpdatedEvent>(), default),
                Times.Never);
        }

        [Fact]
        public async Task UpdateReviewAsync_WithNonExistentReview_ThrowsKeyNotFoundException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(reviewId, userId, dto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Review not found");
        }

        [Fact]
        public async Task UpdateReviewAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var wrongUserId = Guid.NewGuid();
            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(review.Id, wrongUserId, dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You can only update your own reviews");
        }

        [Fact]
        public async Task UpdateReviewAsync_WithDeletedReview_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateDeletedReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot update deleted review");
        }

        [Fact]
        public async Task UpdateReviewAsync_WithHiddenReview_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateHiddenReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot update hidden review. Contact support.");
        }

        [Fact]
        public async Task UpdateReviewAsync_AfterDeadline_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateOldReview(20);
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var dto = ReviewTestData.CreateValidReviewUpdateDto();

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(review.Id, review.UserId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review can only be updated within 14 days after creation");
        }

        #endregion

        #region SoftDeleteReviewAsync Tests

        [Fact]
        public async Task SoftDeleteReviewAsync_ByOwner_DeletesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Act
            await _reviewService.SoftDeleteReviewAsync(review.Id, review.UserId, isAdmin: false);

            // Assert
            var deletedReview = await _dbContext.Reviews.FindAsync(review.Id);
            deletedReview!.Status.Should().Be(ReviewStatus.Deleted);
            deletedReview.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteReviewAsync_PublishesEvents()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Act
            await _reviewService.SoftDeleteReviewAsync(review.Id, review.UserId, isAdmin: false);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewDeletedEvent>(), default),
                Times.Once);

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewStatusChangedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task SoftDeleteReviewAsync_ByAdmin_DeletesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var adminId = Guid.NewGuid();

            // Act
            await _reviewService.SoftDeleteReviewAsync(review.Id, adminId, isAdmin: true);

            // Assert
            var deletedReview = await _dbContext.Reviews.FindAsync(review.Id);
            deletedReview!.Status.Should().Be(ReviewStatus.Deleted);
        }

        [Fact]
        public async Task SoftDeleteReviewAsync_ByWrongUserWithoutAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var wrongUserId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _reviewService.SoftDeleteReviewAsync(review.Id, wrongUserId, isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You can only delete your own reviews");
        }

        [Fact]
        public async Task SoftDeleteReviewAsync_AlreadyDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateDeletedReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _reviewService.SoftDeleteReviewAsync(review.Id, review.UserId, isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review is already deleted");
        }

        #endregion

        #region DeleteReviewAsync Tests

        [Fact]
        public async Task DeleteReviewAsync_PermanentlyDeletesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Act
            await _reviewService.DeleteReviewAsync(review.Id);

            // Assert
            var deletedReview = await _dbContext.Reviews.FindAsync(review.Id);
            deletedReview.Should().BeNull();
        }

        [Fact]
        public async Task DeleteReviewAsync_PublishesReviewDeletedEvent()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Act
            await _reviewService.DeleteReviewAsync(review.Id);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewDeletedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_WithNonExistentReview_ThrowsKeyNotFoundException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _reviewService.DeleteReviewAsync(reviewId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Review not found");
        }

        #endregion

        #region ModerateReviewAsync Tests

        [Fact]
        public async Task ModerateReviewAsync_HideAction_HidesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("hide");

            // Act
            await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            var moderatedReview = await _dbContext.Reviews.FindAsync(review.Id);
            moderatedReview!.Status.Should().Be(ReviewStatus.Hidden);
            moderatedReview.ModeratedBy.Should().Be(moderatorId);
            moderatedReview.ModeratedAt.Should().NotBeNull();
            moderatedReview.ModerationNotes.Should().Be(dto.ModerationNotes);
        }

        [Fact]
        public async Task ModerateReviewAsync_PublishAction_PublishesReview()
        {
            // Arrange
            var review = ReviewTestData.CreateHiddenReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("publish");

            // Act
            await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            var moderatedReview = await _dbContext.Reviews.FindAsync(review.Id);
            moderatedReview!.Status.Should().Be(ReviewStatus.Published);
        }

        [Fact]
        public async Task ModerateReviewAsync_PublishesReviewStatusChangedEvent()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("hide");

            // Act
            await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ReviewStatusChangedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task ModerateReviewAsync_WithInvalidAction_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("invalid");

            // Act
            Func<Task> act = async () => await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid moderation action: invalid. Use 'Hide' or 'Publish'");
        }

        [Fact]
        public async Task ModerateReviewAsync_HideAlreadyHidden_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateHiddenReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("hide");

            // Act
            Func<Task> act = async () => await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review is already hidden");
        }

        [Fact]
        public async Task ModerateReviewAsync_PublishAlreadyPublished_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateValidReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("publish");

            // Act
            Func<Task> act = async () => await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Review is already published");
        }

        [Fact]
        public async Task ModerateReviewAsync_WithDeletedReview_ThrowsInvalidOperationException()
        {
            // Arrange
            var review = ReviewTestData.CreateDeletedReview();
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            var moderatorId = Guid.NewGuid();
            var dto = ReviewTestData.CreateModerationDto("hide");

            // Act
            Func<Task> act = async () => await _reviewService.ModerateReviewAsync(review.Id, moderatorId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot moderate deleted review");
        }

        #endregion
    }
}