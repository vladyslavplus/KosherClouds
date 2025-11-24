using KosherClouds.ServiceDefaults.Redis;
using Moq;

namespace KosherClouds.CartService.UnitTests.Helpers
{
    public static class MockRedisCacheFactory
    {
        public static Mock<IRedisCacheService> Create()
        {
            return new Mock<IRedisCacheService>();
        }

        public static void SetupGetData<T>(
            this Mock<IRedisCacheService> mock,
            string key,
            T? data)
        {
            mock.Setup(x => x.GetDataAsync<T>(key))
                .ReturnsAsync(data);
        }

        public static void SetupSetData<T>(
            this Mock<IRedisCacheService> mock,
            string key)
        {
            mock.Setup(x => x.SetDataAsync(key, It.IsAny<T>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);
        }

        public static void SetupRemoveData(
            this Mock<IRedisCacheService> mock,
            string key)
        {
            mock.Setup(x => x.RemoveDataAsync(key))
                .Returns(Task.CompletedTask);
        }

        public static void VerifyGetDataCalled<T>(
            this Mock<IRedisCacheService> mock,
            string key,
            Times times)
        {
            mock.Verify(
                x => x.GetDataAsync<T>(key),
                times);
        }

        public static void VerifySetDataCalled<T>(
            this Mock<IRedisCacheService> mock,
            string key,
            Times times)
        {
            mock.Verify(
                x => x.SetDataAsync(key, It.IsAny<T>(), It.IsAny<TimeSpan?>()),
                times);
        }

        public static void VerifyRemoveDataCalled(
            this Mock<IRedisCacheService> mock,
            string key,
            Times times)
        {
            mock.Verify(
                x => x.RemoveDataAsync(key),
                times);
        }
    }
}