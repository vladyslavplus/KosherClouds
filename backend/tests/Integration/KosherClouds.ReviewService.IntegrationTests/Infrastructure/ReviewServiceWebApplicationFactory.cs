using KosherClouds.ReviewService.Data;
using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Services.External;
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

namespace KosherClouds.ReviewService.IntegrationTests.Infrastructure
{
    public class ReviewServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;

        public ReviewServiceWebApplicationFactory()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("reviewservice_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());
            builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
            builder.UseSetting("Jwt:Issuer", "KosherCloudsTestIssuer");
            builder.UseSetting("Jwt:Audience", "KosherCloudsTestAudience");
            builder.UseSetting("ServiceUrls:OrderService", "http://localhost:9999");
            builder.UseSetting("ServiceUrls:UserService", "http://localhost:9998");

            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ReviewDbContext>>();
                services.RemoveAll<ReviewDbContext>();

                services.AddDbContext<ReviewDbContext>(options =>
                    options.UseNpgsql(_postgresContainer.GetConnectionString()));

                services.RemoveAll<IOrderApiClient>();
                services.AddSingleton<IOrderApiClient, MockOrderApiClient>();

                services.RemoveAll<IUserApiClient>();
                services.AddSingleton<IUserApiClient, MockUserApiClient>();

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

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    public class MockOrderApiClient : IOrderApiClient
    {
        private static readonly List<OrderDto> _orders = new();

        public Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            return Task.FromResult(order);
        }

        public Task<List<OrderDto>> GetPaidOrdersForUserAsync(Guid userId, int daysBack, CancellationToken cancellationToken = default)
        {
            var minDate = DateTimeOffset.UtcNow.AddDays(-daysBack);
            var orders = _orders
                .Where(o => o.UserId == userId && o.Status == "Paid" && o.CreatedAt >= minDate)
                .ToList();
            return Task.FromResult(orders);
        }

        public static Guid CreateMockOrder(Guid userId, List<OrderItemDto> items)
        {
            var orderId = Guid.NewGuid();
            var order = new OrderDto
            {
                Id = orderId,
                UserId = userId,
                Status = "Paid",
                TotalAmount = items.Sum(i => i.UnitPriceSnapshot * i.Quantity),
                Items = items,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _orders.Add(order);
            return orderId;
        }

        public static void ClearOrders()
        {
            _orders.Clear();
        }
    }

    public class MockUserApiClient : IUserApiClient
    {
        private static readonly Dictionary<Guid, UserDto> _users = new();

        public Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<Dictionary<Guid, string>> GetUserNamesByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, string>();
            foreach (var userId in userIds)
            {
                if (_users.TryGetValue(userId, out var user))
                {
                    var displayName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                        ? $"{user.FirstName} {user.LastName}"
                        : user.UserName ?? "Unknown User";
                    result[userId] = displayName;
                }
            }
            return Task.FromResult(result);
        }

        public static void AddMockUser(Guid userId, string firstName, string lastName, string userName)
        {
            _users[userId] = new UserDto
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                UserName = userName
            };
        }

        public static void ClearUsers()
        {
            _users.Clear();
        }
    }
}