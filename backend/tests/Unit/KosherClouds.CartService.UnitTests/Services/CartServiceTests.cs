using FluentAssertions;
using KosherClouds.CartService.Entities;
using KosherClouds.CartService.UnitTests.Helpers;
using KosherClouds.ServiceDefaults.Redis;
using Microsoft.Extensions.Logging;
using Moq;
using CartServiceClass = KosherClouds.CartService.Services.CartService;

namespace KosherClouds.CartService.UnitTests.Services
{
    public class CartServiceTests : IDisposable
    {
        private readonly Mock<IRedisCacheService> _cacheMock;
        private readonly Mock<ILogger<CartServiceClass>> _loggerMock;
        private readonly CartServiceClass _cartService;
        private bool _disposed;

        public CartServiceTests()
        {
            _cacheMock = MockRedisCacheFactory.Create();
            _loggerMock = new Mock<ILogger<CartServiceClass>>();

            _cartService = new CartServiceClass(
                _cacheMock.Object,
                _loggerMock.Object);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup if needed
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static string GetCartKey(Guid userId) => $"cart:{userId}";

        #region GetCartDetailsAsync Tests

        [Fact]
        public async Task GetCartDetailsAsync_WithExistingCart_ReturnsCartDetails()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var cart = CartTestData.CreateCartWithItems(userId, 3);
            var key = GetCartKey(userId);

            _cacheMock.SetupGetData(key, cart);

            // Act
            var result = await _cartService.GetCartDetailsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Items.Should().HaveCount(3);
            result.Items.Should().AllSatisfy(item =>
            {
                item.ProductId.Should().NotBeEmpty();
                item.Quantity.Should().BeGreaterThan(0);
            });

            _cacheMock.VerifyGetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task GetCartDetailsAsync_WithNonExistingCart_ReturnsEmptyCart()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var key = GetCartKey(userId);

            _cacheMock.SetupGetData<ShoppingCart>(key, null);

            // Act
            var result = await _cartService.GetCartDetailsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Items.Should().BeEmpty();

            _cacheMock.VerifyGetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task GetCartDetailsAsync_MapsCartItemsCorrectly()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var quantity = 5;

            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, quantity));

            var key = GetCartKey(userId);
            _cacheMock.SetupGetData(key, cart);

            // Act
            var result = await _cartService.GetCartDetailsAsync(userId);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items[0].ProductId.Should().Be(productId);
            result.Items[0].Quantity.Should().Be(quantity);
        }

        [Fact]
        public async Task GetCartDetailsAsync_WithManyItems_ReturnsAllItems()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var cart = CartTestData.CreateCartWithItems(userId, 50);
            var key = GetCartKey(userId);

            _cacheMock.SetupGetData(key, cart);

            // Act
            var result = await _cartService.GetCartDetailsAsync(userId);

            // Assert
            result.Items.Should().HaveCount(50);
        }

        #endregion

        #region AddOrUpdateItemAsync Tests

        [Fact]
        public async Task AddOrUpdateItemAsync_WithNewProduct_AddsProductToCart()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var addDto = CartTestData.CreateCartItemAddDto(productId, 3);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].ProductId.Should().Be(productId);
            result.Items[0].Quantity.Should().Be(3);

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithExistingProduct_IncreasesQuantity()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 2));

            var addDto = CartTestData.CreateCartItemAddDto(productId, 3);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart ?? cart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].ProductId.Should().Be(productId);
            result.Items[0].Quantity.Should().Be(5);

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithNegativeQuantity_DecreasesQuantity()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 5));

            var addDto = CartTestData.CreateCartItemAddDto(productId, -2);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart ?? cart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].ProductId.Should().Be(productId);
            result.Items[0].Quantity.Should().Be(3);

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithQuantityResultingInZero_RemovesProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 3));

            var addDto = CartTestData.CreateCartItemAddDto(productId, -3);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart ?? cart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithQuantityResultingInNegative_RemovesProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 2));

            var addDto = CartTestData.CreateCartItemAddDto(productId, -5);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart ?? cart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithZeroQuantityForNewProduct_DoesNotAddProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var addDto = CartTestData.CreateCartItemAddDto(productId, 0);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithNegativeQuantityForNewProduct_DoesNotAddProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var addDto = CartTestData.CreateCartItemAddDto(productId, -5);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithMultipleProducts_MaintainsOtherProducts()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId1, 2));
            cart.Items.Add(CartTestData.CreateCartItem(productId2, 3));

            var addDto = CartTestData.CreateCartItemAddDto(productId1, 1);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart ?? cart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().Contain(i => i.ProductId == productId1 && i.Quantity == 3);
            result.Items.Should().Contain(i => i.ProductId == productId2 && i.Quantity == 3);

            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_SetsCacheTtlCorrectly()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var addDto = CartTestData.CreateCartItemAddDto(productId, 1);
            var key = GetCartKey(userId);

            TimeSpan? capturedTtl = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync((ShoppingCart?)null);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => capturedTtl = t)
                .Returns(Task.CompletedTask);

            // Act
            await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            capturedTtl.Should().NotBeNull();
            capturedTtl.Should().Be(TimeSpan.FromMinutes(30));
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_WithLargeQuantity_AddsSuccessfully()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var addDto = CartTestData.CreateCartItemAddDto(productId, 9999);
            var key = GetCartKey(userId);

            ShoppingCart? savedCart = null;

            _cacheMock.Setup(x => x.GetDataAsync<ShoppingCart>(key))
                .ReturnsAsync(() => savedCart);

            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => savedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddOrUpdateItemAsync(userId, addDto);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items[0].Quantity.Should().Be(9999);
        }

        #endregion

        #region RemoveItemAsync Tests

        [Fact]
        public async Task RemoveItemAsync_WithExistingProduct_RemovesProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 3));

            var key = GetCartKey(userId);

            _cacheMock.SetupGetData(key, cart);
            _cacheMock.SetupSetData<ShoppingCart>(key);

            // Act
            await _cartService.RemoveItemAsync(userId, productId);

            // Assert
            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Once());
        }

        [Fact]
        public async Task RemoveItemAsync_WithNonExistingProduct_DoesNotUpdateCache()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var otherProductId = Guid.NewGuid();

            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(otherProductId, 3));

            var key = GetCartKey(userId);

            _cacheMock.SetupGetData(key, cart);
            _cacheMock.SetupSetData<ShoppingCart>(key);

            // Act
            await _cartService.RemoveItemAsync(userId, productId);

            // Assert
            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Never());
        }

        [Fact]
        public async Task RemoveItemAsync_WithEmptyCart_DoesNothing()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var key = GetCartKey(userId);

            _cacheMock.SetupGetData<ShoppingCart>(key, null);

            // Act
            await _cartService.RemoveItemAsync(userId, productId);

            // Assert
            _cacheMock.VerifySetDataCalled<ShoppingCart>(key, Times.Never());
        }

        [Fact]
        public async Task RemoveItemAsync_WithMultipleProducts_RemovesOnlySpecifiedProduct()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();

            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId1, 2));
            cart.Items.Add(CartTestData.CreateCartItem(productId2, 3));
            cart.Items.Add(CartTestData.CreateCartItem(productId3, 1));

            var key = GetCartKey(userId);

            ShoppingCart? capturedCart = null;
            _cacheMock.SetupGetData(key, cart);
            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => capturedCart = c)
                .Returns(Task.CompletedTask);

            // Act
            await _cartService.RemoveItemAsync(userId, productId2);

            // Assert
            capturedCart.Should().NotBeNull();
            capturedCart!.Items.Should().HaveCount(2);
            capturedCart.Items.Should().Contain(i => i.ProductId == productId1);
            capturedCart.Items.Should().Contain(i => i.ProductId == productId3);
            capturedCart.Items.Should().NotContain(i => i.ProductId == productId2);
        }

        [Fact]
        public async Task RemoveItemAsync_UpdatesCacheTtl()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var productId = Guid.NewGuid();
            var cart = CartTestData.CreateEmptyCart(userId);
            cart.Items.Add(CartTestData.CreateCartItem(productId, 3));

            var key = GetCartKey(userId);

            TimeSpan? capturedTtl = null;

            _cacheMock.SetupGetData(key, cart);
            _cacheMock.Setup(x => x.SetDataAsync(key, It.IsAny<ShoppingCart>(), It.IsAny<TimeSpan?>()))
                .Callback<string, ShoppingCart, TimeSpan?>((k, c, t) => capturedTtl = t)
                .Returns(Task.CompletedTask);

            // Act
            await _cartService.RemoveItemAsync(userId, productId);

            // Assert
            capturedTtl.Should().Be(TimeSpan.FromMinutes(30));
        }

        #endregion

        #region ClearCartAsync Tests

        [Fact]
        public async Task ClearCartAsync_RemovesCartFromCache()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var key = GetCartKey(userId);

            _cacheMock.SetupRemoveData(key);

            // Act
            await _cartService.ClearCartAsync(userId);

            // Assert
            _cacheMock.VerifyRemoveDataCalled(key, Times.Once());
        }

        [Fact]
        public async Task ClearCartAsync_WithDifferentUsers_ClearsCorrectCart()
        {
            // Arrange
            var userId1 = CartTestData.CreateUserId();
            var userId2 = CartTestData.CreateUserId();
            var key1 = GetCartKey(userId1);
            var key2 = GetCartKey(userId2);

            _cacheMock.SetupRemoveData(key1);
            _cacheMock.SetupRemoveData(key2);

            // Act
            await _cartService.ClearCartAsync(userId1);

            // Assert
            _cacheMock.VerifyRemoveDataCalled(key1, Times.Once());
            _cacheMock.VerifyRemoveDataCalled(key2, Times.Never());
        }

        [Fact]
        public async Task ClearCartAsync_MultipleTimes_CallsRemoveEachTime()
        {
            // Arrange
            var userId = CartTestData.CreateUserId();
            var key = GetCartKey(userId);

            _cacheMock.SetupRemoveData(key);

            // Act
            await _cartService.ClearCartAsync(userId);
            await _cartService.ClearCartAsync(userId);
            await _cartService.ClearCartAsync(userId);

            // Assert
            _cacheMock.VerifyRemoveDataCalled(key, Times.Exactly(3));
        }

        #endregion
    }
}