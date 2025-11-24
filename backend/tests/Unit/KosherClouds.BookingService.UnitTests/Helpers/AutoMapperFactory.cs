using AutoMapper;
using KosherClouds.BookingService.Mapping;

namespace KosherClouds.BookingService.UnitTests.Helpers
{
    public static class AutoMapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookingProfile>();
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
