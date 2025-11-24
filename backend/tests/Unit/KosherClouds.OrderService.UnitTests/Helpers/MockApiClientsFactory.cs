using KosherClouds.OrderService.DTOs.External;
using KosherClouds.OrderService.Services.External;
using Moq;

namespace KosherClouds.OrderService.UnitTests.Helpers
{
    public static class MockApiClientsFactory
    {
        public static Mock<ICartApiClient> CreateCartApiClient()
        {
            return new Mock<ICartApiClient>();
        }

        public static Mock<IProductApiClient> CreateProductApiClient()
        {
            return new Mock<IProductApiClient>();
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
    }
}