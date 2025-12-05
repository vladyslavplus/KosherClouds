using AutoMapper;
using FluentAssertions;
using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.Services;
using KosherClouds.OrderService.UnitTests.Helpers;

namespace KosherClouds.OrderService.UnitTests.Services
{
    public class OrderItemServiceTests : IDisposable
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly OrderItemService _orderItemService;
        private bool _disposed;

        public OrderItemServiceTests()
        {
            _dbContext = MockOrderDbContextFactory.Create();
            _mapper = AutoMapperFactory.Create();

            _orderItemService = new OrderItemService(_dbContext, _mapper);
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

        #region GetItemsByOrderIdAsync Tests

        [Fact]
        public async Task GetItemsByOrderIdAsync_WithValidOrderId_ReturnsAllItems()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var item1 = OrderTestData.CreateValidOrderItem(orderId);
            item1.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2);

            var item2 = OrderTestData.CreateValidOrderItem(orderId);
            item2.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1);

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddRangeAsync(item1, item2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetItemsByOrderIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultList = result.ToList();
            resultList[0].Id.Should().Be(item1.Id);
            resultList[1].Id.Should().Be(item2.Id);
        }

        [Fact]
        public async Task GetItemsByOrderIdAsync_WithNoItems_ReturnsEmptyList()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = await _orderItemService.GetItemsByOrderIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetItemsByOrderIdAsync_WithMultipleOrders_ReturnsOnlyRequestedOrderItems()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();

            var order1 = OrderTestData.CreateValidOrder();
            order1.Id = orderId1;
            order1.Items.Clear();

            var order2 = OrderTestData.CreateValidOrder();
            order2.Id = orderId2;
            order2.Items.Clear();

            var item1 = OrderTestData.CreateValidOrderItem(orderId1);
            var item2 = OrderTestData.CreateValidOrderItem(orderId1);
            var item3 = OrderTestData.CreateValidOrderItem(orderId2);

            await _dbContext.Orders.AddRangeAsync(order1, order2);
            await _dbContext.OrderItems.AddRangeAsync(item1, item2, item3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetItemsByOrderIdAsync(orderId1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultIds = result.Select(r => r.Id).ToList();
            resultIds.Should().Contain(item1.Id);
            resultIds.Should().Contain(item2.Id);
            resultIds.Should().NotContain(item3.Id);
        }

        [Fact]
        public async Task GetItemsByOrderIdAsync_OrdersByCreatedAt()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var item1 = OrderTestData.CreateValidOrderItem(orderId);
            item1.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

            var item2 = OrderTestData.CreateValidOrderItem(orderId);
            item2.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5);

            var item3 = OrderTestData.CreateValidOrderItem(orderId);
            item3.CreatedAt = DateTimeOffset.UtcNow;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddRangeAsync(item1, item2, item3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetItemsByOrderIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            var resultList = result.ToList();
            resultList[0].CreatedAt.Should().BeBefore(resultList[1].CreatedAt);
            resultList[1].CreatedAt.Should().BeBefore(resultList[2].CreatedAt);
        }

        [Fact]
        public async Task GetOrderItemByIdAsync_WithUkrainianProductName_ReturnsCorrectly()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateOrderItemWithUkrainianName(orderId);

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetOrderItemByIdAsync(orderItem.Id);

            // Assert
            result.Should().NotBeNull();
            result!.ProductNameSnapshotUk.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region GetOrderItemByIdAsync Tests

        [Fact]
        public async Task GetOrderItemByIdAsync_WithValidId_ReturnsOrderItem()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetOrderItemByIdAsync(orderItem.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(orderItem.Id);
            result.ProductId.Should().Be(orderItem.ProductId);
            result.Quantity.Should().Be(orderItem.Quantity);
            result.UnitPriceSnapshot.Should().Be(orderItem.UnitPriceSnapshot);
            result.ProductNameSnapshot.Should().Be(orderItem.ProductNameSnapshot);
        }

        [Fact]
        public async Task GetOrderItemByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _orderItemService.GetOrderItemByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderItemByIdAsync_VerifiesLineTotal()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            orderItem.UnitPriceSnapshot = 50m;
            orderItem.Quantity = 3;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderItemService.GetOrderItemByIdAsync(orderItem.Id);

            // Assert
            result.Should().NotBeNull();
            result!.LineTotal.Should().Be(150m);
        }

        #endregion

        #region UpdateOrderItemQuantityAsync Tests

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_WithValidData_UpdatesQuantity()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            orderItem.Quantity = 2;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            var newQuantity = 5;

            // Act
            await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, newQuantity);

            // Assert
            var updatedItem = await _dbContext.OrderItems.FindAsync(orderItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.Quantity.Should().Be(newQuantity);
            updatedItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_WithNonExistentItem_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var newQuantity = 5;

            // Act
            Func<Task> act = async () => await _orderItemService.UpdateOrderItemQuantityAsync(nonExistentId, newQuantity);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"OrderItem with ID '{nonExistentId}' not found.");
        }

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_UpdatesTimestamp()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            var originalUpdatedAt = DateTimeOffset.UtcNow.AddHours(-1);
            orderItem.UpdatedAt = originalUpdatedAt;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, 10);

            // Assert
            var updatedItem = await _dbContext.OrderItems.FindAsync(orderItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
            updatedItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_WithZeroQuantity_UpdatesSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            orderItem.Quantity = 5;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, 0);

            // Assert
            var updatedItem = await _dbContext.OrderItems.FindAsync(orderItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.Quantity.Should().Be(0);
        }

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_WithNegativeQuantity_UpdatesWithNegativeValue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            orderItem.Quantity = 5;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, -1);

            // Assert
            var updatedItem = await _dbContext.OrderItems.FindAsync(orderItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.Quantity.Should().Be(-1);
        }

        [Fact]
        public async Task UpdateOrderItemQuantityAsync_WithLargeQuantity_UpdatesSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = OrderTestData.CreateValidOrder();
            order.Id = orderId;
            order.Items.Clear();

            var orderItem = OrderTestData.CreateValidOrderItem(orderId);
            orderItem.Quantity = 1;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();

            var newQuantity = 1000;

            // Act
            await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, newQuantity);

            // Assert
            var updatedItem = await _dbContext.OrderItems.FindAsync(orderItem.Id);
            updatedItem!.Quantity.Should().Be(newQuantity);
        }

        #endregion
    }
}