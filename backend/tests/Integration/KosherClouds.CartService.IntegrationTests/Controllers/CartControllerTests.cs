using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.IntegrationTests.Infrastructure;

namespace KosherClouds.CartService.IntegrationTests.Controllers
{
    public class CartControllerTests : IClassFixture<CartServiceWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CartControllerTests(CartServiceWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCart_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCart_WithAuthentication_ShouldReturnEmptyCart()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var response = await _client.GetAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart.Should().NotBeNull();
            cart!.UserId.Should().Be(userId);
            cart.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task AddOrUpdateItem_WithValidData_ShouldAddItemToCart()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId = Guid.NewGuid();
            var addDto = new CartItemAddDto
            {
                ProductId = productId,
                Quantity = 3
            };

            var response = await _client.PostAsJsonAsync("/api/cart/items", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(1);
            cart.Items[0].ProductId.Should().Be(productId);
            cart.Items[0].Quantity.Should().Be(3);
        }

        [Fact]
        public async Task AddOrUpdateItem_ExistingProduct_ShouldUpdateQuantity()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId = Guid.NewGuid();
            var addDto = new CartItemAddDto
            {
                ProductId = productId,
                Quantity = 2
            };

            await _client.PostAsJsonAsync("/api/cart/items", addDto);
            var response = await _client.PostAsJsonAsync("/api/cart/items", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(1);
            cart.Items[0].ProductId.Should().Be(productId);
            cart.Items[0].Quantity.Should().Be(4);
        }

        [Fact]
        public async Task AddOrUpdateItem_WithNegativeQuantity_ShouldRemoveItem()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId = Guid.NewGuid();
            var addDto = new CartItemAddDto
            {
                ProductId = productId,
                Quantity = 2
            };

            await _client.PostAsJsonAsync("/api/cart/items", addDto);

            var updateDto = new CartItemAddDto
            {
                ProductId = productId,
                Quantity = -5
            };

            var response = await _client.PostAsJsonAsync("/api/cart/items", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart.Should().NotBeNull();
            cart!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task AddOrUpdateItem_MultipleProducts_ShouldAddAllItems()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var product1 = Guid.NewGuid();
            var product2 = Guid.NewGuid();
            var product3 = Guid.NewGuid();

            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = product1,
                Quantity = 1
            });

            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = product2,
                Quantity = 2
            });

            var response = await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = product3,
                Quantity = 3
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(3);
            cart.Items.Sum(i => i.Quantity).Should().Be(6);
        }

        [Fact]
        public async Task AddOrUpdateItem_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var addDto = new CartItemAddDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 1
            };

            var response = await _client.PostAsJsonAsync("/api/cart/items", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RemoveItem_ExistingProduct_ShouldRemoveFromCart()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId = Guid.NewGuid();
            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = productId,
                Quantity = 2
            });

            var response = await _client.DeleteAsync($"/api/cart/items/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync("/api/cart");
            var cart = await getResponse.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveItem_NonExistentProduct_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/cart/items/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RemoveItem_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var productId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/cart/items/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ClearCart_WithItems_ShouldRemoveAllItems()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 1
            });

            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 2
            });

            var response = await _client.DeleteAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync("/api/cart");
            var cart = await getResponse.Content.ReadFromJsonAsync<ShoppingCartDto>();
            cart!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task ClearCart_EmptyCart_ShouldReturnNoContent()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var response = await _client.DeleteAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ClearCart_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.DeleteAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Cart_ShouldIsolateUserData()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            var product1 = Guid.NewGuid();
            var product2 = Guid.NewGuid();

            _client.AddAuthorizationHeader(user1);
            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = product1,
                Quantity = 1
            });

            _client.AddAuthorizationHeader(user2);
            await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto
            {
                ProductId = product2,
                Quantity = 2
            });

            _client.AddAuthorizationHeader(user1);
            var user1Response = await _client.GetAsync("/api/cart");
            var user1Cart = await user1Response.Content.ReadFromJsonAsync<ShoppingCartDto>();

            _client.AddAuthorizationHeader(user2);
            var user2Response = await _client.GetAsync("/api/cart");
            var user2Cart = await user2Response.Content.ReadFromJsonAsync<ShoppingCartDto>();

            user1Cart!.Items.Should().HaveCount(1);
            user1Cart.Items[0].ProductId.Should().Be(product1);
            user1Cart.Items[0].Quantity.Should().Be(1);

            user2Cart!.Items.Should().HaveCount(1);
            user2Cart.Items[0].ProductId.Should().Be(product2);
            user2Cart.Items[0].Quantity.Should().Be(2);
        }
    }
}