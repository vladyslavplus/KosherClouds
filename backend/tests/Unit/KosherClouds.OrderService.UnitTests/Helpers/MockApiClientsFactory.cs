using KosherClouds.OrderService.DTOs.External;
using KosherClouds.OrderService.Services.External;
using Moq;

namespace KosherClouds.OrderService.UnitTests.Helpers
{
    public static class MockApiClientsFactory
    {
        public static Mock<ICartApiClient> CreateCartApiClient()
        {
            var mock = new Mock<ICartApiClient>();

            mock.Setup(x => x.GetCartAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CartItemDto>());

            mock.Setup(x => x.ClearCartAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        public static Mock<IProductApiClient> CreateProductApiClient()
        {
            var mock = new Mock<IProductApiClient>();

            mock.Setup(x => x.GetProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductInfoDto?)null);

            return mock;
        }

        public static Mock<IUserApiClient> CreateUserApiClient()
        {
            var mock = new Mock<IUserApiClient>();

            mock.Setup(x => x.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserInfoDto?)null);

            return mock;
        }

        public static void SetupGetCart(
            this Mock<ICartApiClient> mock,
            Guid userId,
            List<CartItemDto>? cartItems)
        {
            mock.Setup(x => x.GetCartAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cartItems!);
        }

        public static void SetupClearCart(
            this Mock<ICartApiClient> mock,
            Guid userId)
        {
            mock.Setup(x => x.ClearCartAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        public static void SetupGetProduct(
            this Mock<IProductApiClient> mock,
            Guid productId,
            ProductInfoDto? product)
        {
            mock.Setup(x => x.GetProductAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product!);
        }

        public static void SetupGetProducts(
            this Mock<IProductApiClient> mock,
            Dictionary<Guid, ProductInfoDto?> products)
        {
            foreach (var kvp in products)
            {
                mock.SetupGetProduct(kvp.Key, kvp.Value);
            }
        }

        public static void SetupGetUser(
            this Mock<IUserApiClient> mock,
            Guid userId,
            UserInfoDto? user)
        {
            mock.Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user!);
        }

        public static void SetupGetUsers(
            this Mock<IUserApiClient> mock,
            Dictionary<Guid, UserInfoDto?> users)
        {
            foreach (var kvp in users)
            {
                mock.SetupGetUser(kvp.Key, kvp.Value);
            }
        }
    }
}