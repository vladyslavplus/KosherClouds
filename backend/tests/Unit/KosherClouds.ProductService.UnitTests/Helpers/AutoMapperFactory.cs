using AutoMapper;
using KosherClouds.ProductService.Mapping;

namespace KosherClouds.ProductService.UnitTests.Helpers
{
    public static class AutoMapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<HookahDetailsProfile>();
            });

            return config.CreateMapper();
        }
    }
}
