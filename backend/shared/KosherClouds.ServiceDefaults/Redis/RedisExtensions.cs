using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KosherClouds.ServiceDefaults.Redis
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection = config.GetConnectionString("redis")
                                   ?? config.GetConnectionString("Redis")
                                   ?? throw new InvalidOperationException(
                                       "Redis connection string not found. Expected 'redis' or 'Redis' in ConnectionStrings.");

                var configuration = ConfigurationOptions.Parse(redisConnection);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectRetry = 3;
                configuration.ConnectTimeout = 5000;
                configuration.SyncTimeout = 5000;

                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            return services;
        }
    }
}
