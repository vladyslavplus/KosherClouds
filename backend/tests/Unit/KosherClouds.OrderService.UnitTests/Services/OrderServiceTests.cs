using AutoMapper;
using FluentAssertions;
using KosherClouds.Contracts.Orders;
using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.DTOs.External;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.Parameters;
using KosherClouds.OrderService.Services.External;
using KosherClouds.OrderService.UnitTests.Helpers;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderServiceClass = KosherClouds.OrderService.Services.OrderService;

namespace KosherClouds.OrderService.UnitTests.Services
{
    public class OrderServiceTests : IDisposable
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<ISortHelperFactory> _sortHelperFactoryMock;
        private readonly Mock<ISortHelper<Order>> _sortHelperMock;
        private readonly Mock<ILogger<OrderServiceClass>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ICartApiClient> _cartApiClientMock;
        private readonly Mock<IProductApiClient> _productApiClientMock;
        private readonly Mock<IUserApiClient> _userApiClientMock;
        private readonly OrderServiceClass _orderService;
        private bool _disposed;

        public OrderServiceTests()
        {
            _dbContext = MockOrderDbContextFactory.Create();
            _mapper = AutoMapperFactory.Create();
            _loggerMock = new Mock<ILogger<OrderServiceClass>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _cartApiClientMock = MockApiClientsFactory.CreateCartApiClient();
            _productApiClientMock = MockApiClientsFactory.CreateProductApiClient();
            _userApiClientMock = MockApiClientsFactory.CreateUserApiClient();

            _sortHelperMock = new Mock<ISortHelper<Order>>();
            _sortHelperMock
                .Setup(x => x.ApplySort(It.IsAny<IQueryable<Order>>(), It.IsAny<string>()))
                .Returns<IQueryable<Order>, string>((query, orderBy) => query);

            _sortHelperFactoryMock = new Mock<ISortHelperFactory>();
            _sortHelperFactoryMock
                .Setup(x => x.Create<Order>())
                .Returns(_sortHelperMock.Object);

            _orderService = new OrderServiceClass(
                _dbContext,
                _mapper,
                _sortHelperFactoryMock.Object,
                _loggerMock.Object,
                _publishEndpointMock.Object,
                _cartApiClientMock.Object,
                _productApiClientMock.Object,
                _userApiClientMock.Object,
                isInMemory: true);
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

        #region GetOrdersAsync Tests

        [Fact]
        public async Task GetOrdersAsync_WithNoFilters_ReturnsAllOrders()
        {
            // Arrange
            var orders = OrderTestData.CreateOrderList(3);
            await _dbContext.Orders.AddRangeAsync(orders);
            await _dbContext.SaveChangesAsync();

            var parameters = OrderTestData.CreateOrderParameters();

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
        }

        [Fact]
        public async Task GetOrdersAsync_WithUserIdFilter_ReturnsMatchingOrders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userOrders = OrderTestData.CreateOrderList(2, userId);
            var otherOrders = OrderTestData.CreateOrderList(1);

            await _dbContext.Orders.AddRangeAsync(userOrders);
            await _dbContext.Orders.AddRangeAsync(otherOrders);
            await _dbContext.SaveChangesAsync();

            var parameters = new OrderParameters
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task GetOrdersAsync_WithStatusFilter_ReturnsMatchingOrders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paidOrder = OrderTestData.CreatePaidOrder(userId);
            var draftOrder = OrderTestData.CreateDraftOrder(userId);

            await _dbContext.Orders.AddRangeAsync(paidOrder, draftOrder);
            await _dbContext.SaveChangesAsync();

            var parameters = new OrderParameters
            {
                Status = OrderStatus.Paid,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public async Task GetOrdersAsync_WithAmountFilter_ReturnsMatchingOrders()
        {
            // Arrange
            var order1 = OrderTestData.CreateValidOrder();
            order1.TotalAmount = 100m;
            var order2 = OrderTestData.CreateValidOrder();
            order2.TotalAmount = 500m;

            await _dbContext.Orders.AddRangeAsync(order1, order2);
            await _dbContext.SaveChangesAsync();

            var parameters = new OrderParameters
            {
                MinTotalAmount = 200m,
                MaxTotalAmount = 600m,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].TotalAmount.Should().Be(500m);
        }

        [Fact]
        public async Task GetOrdersAsync_WithPaymentTypeFilter_ReturnsMatchingOrders()
        {
            // Arrange
            var onlineOrder = OrderTestData.CreateOrderWithPaymentType(PaymentType.Online);
            var onPickupOrder = OrderTestData.CreateOrderWithPaymentType(PaymentType.OnPickup);

            await _dbContext.Orders.AddRangeAsync(onlineOrder, onPickupOrder);
            await _dbContext.SaveChangesAsync();

            var parameters = new OrderParameters
            {
                PaymentType = PaymentType.Online,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].PaymentType.Should().Be(PaymentType.Online);
        }

        [Fact]
        public async Task GetOrdersAsync_WithDateRangeFilter_ReturnsMatchingOrders()
        {
            // Arrange
            var oldOrder = OrderTestData.CreateOldOrder(30);
            var recentOrder = OrderTestData.CreateOldOrder(5);

            await _dbContext.Orders.AddRangeAsync(oldOrder, recentOrder);
            await _dbContext.SaveChangesAsync();

            var parameters = new OrderParameters
            {
                MinOrderDate = DateTimeOffset.UtcNow.AddDays(-10),
                MaxOrderDate = DateTimeOffset.UtcNow,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _orderService.GetOrdersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
        }

        #endregion

        #region GetOrderByIdAsync Tests

        [Fact]
        public async Task GetOrderByIdAsync_WithValidId_ReturnsOrder()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrderByIdAsync(order.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(order.Id);
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _orderService.GetOrderByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateOrderFromCartAsync Tests

        [Fact]
        public async Task CreateOrderFromCartAsync_WithValidCart_CreatesOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();

            var products = new Dictionary<Guid, ProductInfoDto?>();
            foreach (var item in cartItems)
            {
                products[item.ProductId] = OrderTestData.CreateProductInfo(item.ProductId, true);
            }

            var userInfo = OrderTestData.CreateUserInfo(userId);

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _productApiClientMock.SetupGetProducts(products);
            _userApiClientMock.SetupGetUser(userId, userInfo);

            // Act
            var result = await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Status.Should().Be(OrderStatus.Draft);
            result.Items.Should().HaveCount(2);

            _cartApiClientMock.Verify(
                x => x.ClearCartAsync(userId, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithEmptyCart_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _cartApiClientMock.SetupGetCart(userId, new List<CartItemDto>());

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cart is empty");
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithUnavailableProducts_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();

            var products = new Dictionary<Guid, ProductInfoDto?>();
            foreach (var item in cartItems)
            {
                products[item.ProductId] = OrderTestData.CreateProductInfo(item.ProductId, false);
            }

            var userInfo = OrderTestData.CreateUserInfo(userId);

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _productApiClientMock.SetupGetProducts(products);
            _userApiClientMock.SetupGetUser(userId, userInfo);

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No valid products found in cart. All products are unavailable.");
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithSomeUnavailableProducts_CreatesOrderWithAvailableProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();

            var availableProduct = OrderTestData.CreateProductInfo(cartItems[0].ProductId, true);
            var unavailableProduct = OrderTestData.CreateProductInfo(cartItems[1].ProductId, false);

            var userInfo = OrderTestData.CreateUserInfo(userId);

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _productApiClientMock.SetupGetProduct(cartItems[0].ProductId, availableProduct);
            _productApiClientMock.SetupGetProduct(cartItems[1].ProductId, unavailableProduct);
            _userApiClientMock.SetupGetUser(userId, userInfo);
            _cartApiClientMock.SetupClearCart(userId);

            // Act
            var result = await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductId.Should().Be(cartItems[0].ProductId);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithUserWithoutPhoneNumber_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();
            var userInfo = OrderTestData.CreateUserInfoWithoutPhone(userId);

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _userApiClientMock.SetupGetUser(userId, userInfo);

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Phone number is required. Please update your profile.");
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithNonExistentUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _userApiClientMock.SetupGetUser(userId, null);

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to fetch user information");
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_WithUkrainianProductNames_CreatesOrderCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartItems = OrderTestData.CreateCartItems();
            var userInfo = OrderTestData.CreateUserInfo(userId);

            var product1 = OrderTestData.CreateProductInfoWithUkrainianName(cartItems[0].ProductId, true);
            var product2 = OrderTestData.CreateProductInfoWithUkrainianName(cartItems[1].ProductId, true);

            _cartApiClientMock.SetupGetCart(userId, cartItems);
            _userApiClientMock.SetupGetUser(userId, userInfo);
            _productApiClientMock.SetupGetProduct(cartItems[0].ProductId, product1);
            _productApiClientMock.SetupGetProduct(cartItems[1].ProductId, product2);
            _cartApiClientMock.SetupClearCart(userId);

            // Act
            var result = await _orderService.CreateOrderFromCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().AllSatisfy(item =>
                item.ProductNameSnapshotUk.Should().NotBeNullOrEmpty());
        }

        #endregion

        #region CreateDraftOrderAsync Tests

        [Fact]
        public async Task CreateDraftOrderAsync_WithValidData_CreatesDraftOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderDto = OrderTestData.CreateValidOrderCreateDto(userId);

            // Act
            var result = await _orderService.CreateDraftOrderAsync(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Status.Should().Be(OrderStatus.Draft);
            result.TotalAmount.Should().BeGreaterThan(0);
            result.Items.Should().HaveCount(2);

            var savedOrder = await _dbContext.Orders.FindAsync(result.Id);
            savedOrder.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDraftOrderAsync_CalculatesTotalAmountCorrectly()
        {
            // Arrange
            var orderDto = OrderTestData.CreateValidOrderCreateDto();
            var expectedTotal = orderDto.Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);

            // Act
            var result = await _orderService.CreateDraftOrderAsync(orderDto);

            // Assert
            result.TotalAmount.Should().Be(expectedTotal);
        }

        #endregion

        #region ConfirmOrderAsync Tests

        [Fact]
        public async Task ConfirmOrderAsync_WithDraftOrder_ConfirmsOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = OrderTestData.CreateDraftOrder(userId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var confirmDto = OrderTestData.CreateValidOrderConfirmDto();
            confirmDto.PaymentType = PaymentType.OnPickup;

            // Act
            var result = await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(OrderStatus.Pending);
            result.PaymentType.Should().Be(PaymentType.OnPickup);

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithOnlinePayment_DoesNotPublishEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = OrderTestData.CreateDraftOrder(userId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var confirmDto = OrderTestData.CreateValidOrderConfirmDto();
            confirmDto.PaymentType = PaymentType.Online;

            // Act
            var result = await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(OrderStatus.Pending);
            result.PaymentType.Should().Be(PaymentType.Online);

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var confirmDto = OrderTestData.CreateValidOrderConfirmDto();

            // Act
            Func<Task> act = async () => await _orderService.ConfirmOrderAsync(orderId, userId, confirmDto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Order with ID '{orderId}' not found.");
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var order = OrderTestData.CreateDraftOrder(userId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var confirmDto = OrderTestData.CreateValidOrderConfirmDto();

            // Act
            Func<Task> act = async () => await _orderService.ConfirmOrderAsync(order.Id, wrongUserId, confirmDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"User {wrongUserId} is not authorized to confirm order {order.Id}");
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithNonDraftOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = OrderTestData.CreatePaidOrder(userId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var confirmDto = OrderTestData.CreateValidOrderConfirmDto();

            // Act
            Func<Task> act = async () => await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Only Draft orders can be confirmed. Current status: {order.Status}");
        }

        [Fact]
        public async Task ConfirmOrderAsync_UpdatesContactInformation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = OrderTestData.CreateDraftOrder(userId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var confirmDto = new OrderConfirmDto
            {
                ContactName = "John Doe",
                ContactPhone = "+380991234567",
                Notes = "Please call before delivery",
                PaymentType = PaymentType.OnPickup
            };

            _cartApiClientMock.SetupClearCart(userId);

            // Act
            var result = await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

            // Assert
            result.Should().NotBeNull();
            result.ContactName.Should().Be("John Doe");
            result.ContactPhone.Should().Be("+380991234567");
            result.Notes.Should().Be("Please call before delivery");
        }

        #endregion

        #region MarkOrderAsPaidAsync Tests

        [Fact]
        public async Task MarkOrderAsPaidAsync_WithValidOrder_MarksAsPaid()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();
            order.Status = OrderStatus.Pending;
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderService.MarkOrderAsPaidAsync(order.Id);

            // Assert
            var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.Paid);
            updatedOrder.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task MarkOrderAsPaidAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _orderService.MarkOrderAsPaidAsync(orderId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Order {orderId} not found.");
        }

        #endregion

        #region UpdateOrderAsync Tests

        [Fact]
        public async Task UpdateOrderAsync_WithValidData_UpdatesOrder()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var updateDto = OrderTestData.CreateValidOrderUpdateDto();

            // Act
            await _orderService.UpdateOrderAsync(order.Id, updateDto);

            // Assert
            var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.Completed);
            updatedOrder.Notes.Should().Be("Order completed");

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<OrderUpdatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateDto = OrderTestData.CreateValidOrderUpdateDto();

            // Act
            Func<Task> act = async () => await _orderService.UpdateOrderAsync(orderId, updateDto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Order with ID '{orderId}' not found.");
        }

        [Fact]
        public async Task UpdateOrderAsync_WithPartialUpdate_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();
            var originalNotes = order.Notes;
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var updateDto = new OrderUpdateDto
            {
                Status = OrderStatus.Completed
            };

            // Act
            await _orderService.UpdateOrderAsync(order.Id, updateDto);

            // Assert
            var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
            updatedOrder!.Status.Should().Be(OrderStatus.Completed);
            updatedOrder.Notes.Should().Be(originalNotes);
        }

        #endregion

        #region DeleteOrderAsync Tests

        [Fact]
        public async Task DeleteOrderAsync_WithValidOrder_DeletesOrder()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderService.DeleteOrderAsync(order.Id);

            // Assert
            var deletedOrder = await _dbContext.Orders.FindAsync(order.Id);
            deletedOrder.Should().BeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<OrderDeletedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _orderService.DeleteOrderAsync(orderId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Order with ID '{orderId}' not found.");
        }

        #endregion
    }
}