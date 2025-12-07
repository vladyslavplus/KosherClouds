using KosherClouds.BookingService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace KosherClouds.BookingService.IntegrationTests.Infrastructure
{
    public class BookingServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private readonly RabbitMqContainer _rabbitmqContainer;

        public BookingServiceWebApplicationFactory()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("bookingservice_test")
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
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());
            builder.UseSetting("RabbitMq:Host", _rabbitmqContainer.Hostname);
            builder.UseSetting("RabbitMq:Username", "guest");
            builder.UseSetting("RabbitMq:Password", "guest");
            builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
            builder.UseSetting("Jwt:Issuer", "KosherCloudsTestIssuer");
            builder.UseSetting("Jwt:Audience", "KosherCloudsTestAudience");

            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<BookingDbContext>>();
                services.RemoveAll<BookingDbContext>();

                services.AddDbContext<BookingDbContext>(options =>
                    options.UseNpgsql(_postgresContainer.GetConnectionString()));

                services.AddMassTransitTestHarness(x =>
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                });

                services.Configure<JsonOptions>(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await _rabbitmqContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _rabbitmqContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}