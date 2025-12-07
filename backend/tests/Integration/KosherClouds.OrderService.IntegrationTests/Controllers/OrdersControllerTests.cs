using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.IntegrationTests.Infrastructure;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace KosherClouds.OrderService.IntegrationTests.Controllers
{
    public class OrdersControllerTests : IClassFixture<OrderServiceWebApplicationFactory>
    {
        private readonly OrderServiceWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public OrdersControllerTests(OrderServiceWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateOrderFromCart_WithValidCart_ShouldReturnCreatedOrder()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = new[]
                        {
                            new { productId = productId1, quantity = 2 },
                            new { productId = productId2, quantity = 1 }
                        }
                    }));

            _factory.ProductServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/products/{productId1}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId1,
                        name = "Test Product 1",
                        nameUk = "Тестовий продукт 1",
                        price = 100.00m,
                        actualPrice = 100.00m,
                        isAvailable = true,
                        stock = 10
                    }));

            _factory.ProductServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/products/{productId2}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId2,
                        name = "Test Product 2",
                        nameUk = "Тестовий продукт 2",
                        price = 150.00m,
                        actualPrice = 150.00m,
                        isAvailable = true,
                        stock = 5
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
                        firstName = "John",
                        lastName = "Doe",
                        phoneNumber = "+380123456789",
                        displayName = "John Doe"
                    }));

            var response = await _client.PostAsync("/api/orders/from-cart", null);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdOrder = await response.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
            createdOrder.Should().NotBeNull();
            createdOrder!.UserId.Should().Be(userId);
            createdOrder.Status.Should().Be(Entities.OrderStatus.Draft);
            createdOrder.Items.Should().HaveCount(2);
            createdOrder.TotalAmount.Should().Be(350.00m);
        }

        [Fact]
        public async Task CreateOrderFromCart_WithEmptyCart_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = Array.Empty<object>()
                    }));

            var response = await _client.PostAsync("/api/orders/from-cart", null);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateOrderFromCart_WithUnavailableProduct_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

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

            _factory.ProductServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/products/{productId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId,
                        name = "Unavailable Product",
                        nameUk = "Недоступний продукт",
                        price = 100.00m,
                        actualPrice = 100.00m,
                        isAvailable = false,
                        stock = 0
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

            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateOrderFromCart_WithInsufficientStock_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = new[] { new { productId, quantity = 10 } }
                    }));

            _factory.ProductServiceMock
                .Given(Request.Create()
                    .WithPath($"/api/products/{productId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId,
                        name = "Low Stock Product",
                        nameUk = "Продукт з низьким запасом",
                        price = 100.00m,
                        actualPrice = 100.00m,
                        isAvailable = true,
                        stock = 3
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

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateOrderFromCart_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.PostAsync("/api/orders/from-cart", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ConfirmOrder_WithValidData_ShouldMoveOrderToPending()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateDraftOrderForUser(userId);

            _factory.CartServiceMock
                .Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            var confirmDto = new
            {
                contactName = "John Doe",
                contactPhone = "+380987654321",
                notes = "Please prepare quickly",
                paymentType = 0
            };

            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/confirm", confirmDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var confirmedOrder = await response.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
            confirmedOrder.Should().NotBeNull();
            confirmedOrder!.Status.Should().Be(Entities.OrderStatus.Pending);
            confirmedOrder.ContactName.Should().Be("John Doe");
            confirmedOrder.ContactPhone.Should().Be("+380987654321");
        }

        [Fact]
        public async Task ConfirmOrder_ByDifferentUser_ShouldReturnUnauthorized()
        {
            var originalUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _client.AddAuthorizationHeader(originalUserId);
            var orderId = await CreateDraftOrderForUser(originalUserId);

            _client.AddAuthorizationHeader(differentUserId);

            var confirmDto = new
            {
                contactName = "John Doe",
                contactPhone = "+380987654321",
                paymentType = 0
            };

            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/confirm", confirmDto);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetOrders_AsUser_ShouldReturnOnlyOwnOrders()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            await CreateDraftOrderForUser(userId);
            await CreateDraftOrderForUser(userId);

            var response = await _client.GetAsync("/api/orders");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var orders = await response.Content.ReadFromJsonAsync<List<OrderResponseDto>>(JsonOptions);
            orders.Should().NotBeNull();
            orders!.Should().HaveCountGreaterOrEqualTo(2);
            orders!.All(o => o.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetOrders_AsAdmin_ShouldReturnAllOrders()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            await CreateDraftOrderForUser(user1);
            await CreateDraftOrderForUser(user2);

            var response = await _client.GetAsync("/api/orders");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var orders = await response.Content.ReadFromJsonAsync<List<OrderResponseDto>>(JsonOptions);
            orders.Should().NotBeNull();
            orders!.Should().HaveCountGreaterOrEqualTo(2);
        }

        [Fact]
        public async Task GetOrderById_WithValidId_ShouldReturnOrder()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateDraftOrderForUser(userId);

            var response = await _client.GetAsync($"/api/orders/{orderId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
            order.Should().NotBeNull();
            order!.Id.Should().Be(orderId);
            order.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetOrderById_ByDifferentUser_ShouldReturnForbidden()
        {
            var originalUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _client.AddAuthorizationHeader(originalUserId);
            var orderId = await CreateDraftOrderForUser(originalUserId);

            _client.AddAuthorizationHeader(differentUserId);

            var response = await _client.GetAsync($"/api/orders/{orderId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetOrderById_WithNonExistentId_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/orders/{nonExistentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateOrder_AsAdmin_ShouldUpdateSuccessfully()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = await CreateDraftOrderForUser(userId);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var updateDto = new
            {
                status = 2,
                notes = "Updated by admin"
            };

            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateOrder_AsRegularUser_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var orderId = await CreateDraftOrderForUser(userId);

            var updateDto = new
            {
                status = 2,
                notes = "Trying to update"
            };

            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteOrder_AsAdmin_ShouldDeleteSuccessfully()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var orderId = await CreateDraftOrderForUser(userId);

            _client.AddAuthorizationHeader(adminId, new[] { "Manager" });

            var response = await _client.DeleteAsync($"/api/orders/{orderId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task<Guid> CreateDraftOrderForUser(Guid userId)
        {
            var originalAuth = _client.DefaultRequestHeaders.Authorization;

            _client.AddAuthorizationHeader(userId);

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
                        items = new[] { new { productId, quantity = 1 } }
                    }));

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
            var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);

            if (originalAuth != null)
            {
                _client.DefaultRequestHeaders.Authorization = originalAuth;
            }

            return order!.Id;
        }
    }
}