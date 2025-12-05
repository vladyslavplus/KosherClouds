using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Services.External;
using Moq;

namespace KosherClouds.ReviewService.UnitTests.Helpers
{
    public static class MockApiClientsFactory
    {
        public static Mock<IOrderApiClient> CreateOrderApiClient()
        {
            var mock = new Mock<IOrderApiClient>();

            mock.Setup(x => x.GetOrderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDto?)null);

            mock.Setup(x => x.GetPaidOrdersForUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrderDto>());

            return mock;
        }

        public static Mock<IUserApiClient> CreateUserApiClient()
        {
            var mock = new Mock<IUserApiClient>();

            mock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDto?)null);

            mock.Setup(x => x.GetUserNamesByIdsAsync(
                    It.IsAny<List<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, string>());

            return mock;
        }

        public static void SetupGetOrderById(
            this Mock<IOrderApiClient> mock,
            Guid orderId,
            OrderDto? order)
        {
            mock.Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
        }

        public static void SetupGetPaidOrders(
            this Mock<IOrderApiClient> mock,
            Guid userId,
            List<OrderDto> orders)
        {
            mock.Setup(x => x.GetPaidOrdersForUserAsync(
                    userId,
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);
        }

        public static void SetupGetUserById(
            this Mock<IUserApiClient> mock,
            Guid userId,
            UserDto? user)
        {
            mock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }

        public static void SetupGetUserNamesByIds(
            this Mock<IUserApiClient> mock,
            Dictionary<Guid, string> userNames)
        {
            mock.Setup(x => x.GetUserNamesByIdsAsync(
                    It.IsAny<List<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userNames);
        }
    }
}