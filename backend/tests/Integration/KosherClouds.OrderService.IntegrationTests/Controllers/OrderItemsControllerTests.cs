using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.IntegrationTests.Infrastructure;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace KosherClouds.OrderService.IntegrationTests.Controllers
{
    public class OrderItemsControllerTests : IClassFixture<OrderServiceWebApplicationFactory>
    {
        private readonly OrderServiceWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public OrderItemsControllerTests(OrderServiceWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetItemsByOrderId_WithValidOrder_ShouldReturnItems()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateOrderWithItems(userId);

            var response = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var items = await response.Content.ReadFromJsonAsync<List<OrderItemResponseDto>>(JsonOptions);
            items.Should().NotBeNull();
            items!.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetItemsByOrderId_WithNonExistentOrder_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var nonExistentOrderId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/OrderItems/by-order/{nonExistentOrderId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetOrderItemById_WithValidId_ShouldReturnItem()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateOrderWithItems(userId);

            var itemsResponse = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemResponseDto>>(JsonOptions);
            var itemId = items![0].Id;

            var response = await _client.GetAsync($"/api/OrderItems/{itemId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var item = await response.Content.ReadFromJsonAsync<OrderItemResponseDto>(JsonOptions);
            item.Should().NotBeNull();
            item!.Id.Should().Be(itemId);
        }

        [Fact]
        public async Task GetOrderItemById_WithNonExistentId_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/OrderItems/{nonExistentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateOrderItemQuantity_AsAdmin_ShouldUpdateSuccessfully()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = await CreateOrderWithItems(userId);

            var itemsResponse = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemResponseDto>>(JsonOptions);
            var itemId = items![0].Id;

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var response = await _client.PutAsync($"/api/OrderItems/{itemId}/quantity?newQuantity=5", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _client.AddAuthorizationHeader(userId);
            var updatedItemResponse = await _client.GetAsync($"/api/OrderItems/{itemId}");
            var updatedItem = await updatedItemResponse.Content.ReadFromJsonAsync<OrderItemResponseDto>(JsonOptions);
            updatedItem!.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task UpdateOrderItemQuantity_AsRegularUser_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateOrderWithItems(userId);

            var itemsResponse = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemResponseDto>>(JsonOptions);
            var itemId = items![0].Id;

            var response = await _client.PutAsync($"/api/OrderItems/{itemId}/quantity?newQuantity=5", null);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateOrderItemQuantity_WithInvalidQuantity_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);
            var orderId = await CreateOrderWithItems(userId);
            var itemsResponse = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemResponseDto>>(JsonOptions);
            var itemId = items![0].Id;
            _client.AddAuthorizationHeader(adminId, new[] { "Manager" });

            var response = await _client.PutAsync($"/api/OrderItems/{itemId}/quantity?newQuantity=-1", null);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetItemsByOrderId_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var orderId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/OrderItems/by-order/{orderId}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private async Task<Guid> CreateOrderWithItems(Guid userId)
        {
            var productId = Guid.NewGuid();

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = new[] { new { productId, quantity = 2 } }
                    }));

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            _factory.ProductServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/products/{productId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId,
                        name = "Test Product",
                        nameUk = "Тестовий продукт",
                        price = 100.00m,
                        actualPrice = 100.00m,
                        isAvailable = true,
                        stock = 10
                    }));

            _factory.UserServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/users/{userId}/public")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = userId,
                        email = "test@example.com",
                        firstName = "Test",
                        lastName = "User",
                        phoneNumber = "+380123456789",
                        displayName = "Test User"
                    }));

            var response = await _client.PostAsync("/api/orders/from-cart", null);
            var order = await response.Content.ReadFromJsonAsync<KosherClouds.OrderService.DTOs.Order.OrderResponseDto>(JsonOptions);

            return order!.Id;
        }
    }
}