using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace KosherClouds.OrderService.IntegrationTests.Infrastructure
{
    public static class MockHelper
    {
        public static void SetupCartMock(WireMockServer mock, Guid userId, Guid productId, int quantity = 1)
        {
            mock.Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = new[] { new { productId, quantity } }
                    }));
        }

        public static void SetupCartMockMultiple(WireMockServer mock, Guid userId, params (Guid productId, int quantity)[] items)
        {
            var itemsList = items.Select(i => new { productId = i.productId, quantity = i.quantity }).ToArray();

            mock.Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = itemsList
                    }));
        }

        public static void SetupEmptyCartMock(WireMockServer mock, Guid userId)
        {
            mock.Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        userId,
                        items = Array.Empty<object>()
                    }));
        }

        public static void SetupCartClearMock(WireMockServer mock)
        {
            mock.Given(Request.Create()
                    .WithPath("/api/cart")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));
        }

        public static void SetupProductMock(WireMockServer mock, Guid productId, string name = "Test Product",
            decimal price = 100.00m, bool isAvailable = true, int stock = 10)
        {
            mock.Given(Request.Create()
                    .WithPath($"/api/products/{productId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = productId,
                        name,
                        nameUk = name,
                        price,
                        actualPrice = price,
                        isAvailable,
                        stock
                    }));
        }

        public static void SetupUserMock(WireMockServer mock, Guid userId, string email = "test@example.com",
            string firstName = "Test", string lastName = "User", string phoneNumber = "+380123456789")
        {
            mock.Given(Request.Create()
                    .WithPath($"/api/users/{userId}/public")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        id = userId,
                        email,
                        firstName,
                        lastName,
                        phoneNumber,
                        displayName = $"{firstName} {lastName}"
                    }));
        }
    }
}