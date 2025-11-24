using AutoMapper;
using KosherClouds.ReviewService.Mapping;

namespace KosherClouds.ReviewService.UnitTests.Helpers
{
    public static class AutoMapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ReviewMappingProfile>();
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
