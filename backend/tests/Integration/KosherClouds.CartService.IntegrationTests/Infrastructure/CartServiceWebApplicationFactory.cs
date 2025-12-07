using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace KosherClouds.CartService.IntegrationTests.Infrastructure
{
    public class CartServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer;

        public CartServiceWebApplicationFactory()
        {
            _redisContainer = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
            builder.UseSetting("Jwt:Issuer", "KosherCloudsTestIssuer");
            builder.UseSetting("Jwt:Audience", "KosherCloudsTestAudience");
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IConnectionMultiplexer>();

                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var connectionString = _redisContainer.GetConnectionString();
                    return ConnectionMultiplexer.Connect(connectionString);
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}