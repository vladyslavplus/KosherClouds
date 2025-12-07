using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using KosherClouds.OrderService.Data;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using WireMock.Server;
using MassTransit;

namespace KosherClouds.OrderService.IntegrationTests.Infrastructure
{
    public class OrderServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private readonly RabbitMqContainer _rabbitmqContainer;
        private readonly RedisContainer _redisContainer;

        public WireMockServer CartServiceMock { get; private set; } = null!;
        public WireMockServer ProductServiceMock { get; private set; } = null!;
        public WireMockServer UserServiceMock { get; private set; } = null!;

        public OrderServiceWebApplicationFactory()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("orderservice_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .Build();

            _rabbitmqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3-management")
                .WithUsername("guest")
                .WithPassword("guest")
                .WithCleanUp(true)
                .Build();

            _redisContainer = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            CartServiceMock = WireMockServer.Start();
            ProductServiceMock = WireMockServer.Start();
            UserServiceMock = WireMockServer.Start();

            builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());
            builder.UseSetting("RabbitMq:Host", _rabbitmqContainer.Hostname);
            builder.UseSetting("RabbitMq:Username", "guest");
            builder.UseSetting("RabbitMq:Password", "guest");
            builder.UseSetting("ServiceUrls:CartService", CartServiceMock.Url!);
            builder.UseSetting("ServiceUrls:ProductService", ProductServiceMock.Url!);
            builder.UseSetting("ServiceUrls:UserService", UserServiceMock.Url!);
            builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
            builder.UseSetting("Jwt:Issuer", "KosherCloudsTestIssuer");
            builder.UseSetting("Jwt:Audience", "KosherCloudsTestAudience");

            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<OrderDbContext>>();
                services.RemoveAll<OrderDbContext>();

                services.AddDbContext<OrderDbContext>(options =>
                    options.UseNpgsql(_postgresContainer.GetConnectionString()));

                services.AddMassTransitTestHarness(x =>
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await _rabbitmqContainer.StartAsync();
            await _redisContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            CartServiceMock?.Stop();
            ProductServiceMock?.Stop();
            UserServiceMock?.Stop();

            await _postgresContainer.DisposeAsync();
            await _rabbitmqContainer.DisposeAsync();
            await _redisContainer.DisposeAsync();

            await base.DisposeAsync();
        }
    }
}