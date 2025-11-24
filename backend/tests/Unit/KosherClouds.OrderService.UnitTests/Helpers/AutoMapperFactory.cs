using AutoMapper;
using KosherClouds.OrderService.Mapping;

namespace KosherClouds.OrderService.UnitTests.Helpers
{
    public static class AutoMapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrderProfile>();
                cfg.AddProfile<OrderItemProfile>();
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
