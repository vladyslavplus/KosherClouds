using KosherClouds.OrderService.Entities;
using KosherClouds.Common.Seed;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Data.Seed;

public static class OrderSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("OrderSeeder");

        try
        {
            logger.LogInformation("Applying OrderService database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("OrderService migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying OrderService migrations.");
            throw;
        }

        if (!await dbContext.Orders.AnyAsync())
        {
            logger.LogInformation("Starting OrderService data seeding...");

            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = SharedSeedData.ManagerId,
                    Status = "Completed",
                    TotalAmount = 490.00m,
                    Notes = "Delivery to office, please call before arrival",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductFalafelSetId,
                            ProductNameSnapshot = SharedSeedData.ProductFalafelSetName,
                            UnitPriceSnapshot = SharedSeedData.ProductFalafelSetPrice,
                            Quantity = 1,
                            CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-7)
                        },
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductKugelId,
                            ProductNameSnapshot = SharedSeedData.ProductKugelName,
                            UnitPriceSnapshot = SharedSeedData.ProductKugelPrice,
                            Quantity = 2,
                            CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-7)
                        }
                    }
                },

                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = SharedSeedData.UserId,
                    Status = "Pending",
                    TotalAmount = 350.00m,
                    Notes = null,
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
                    UpdatedAt = DateTimeOffset.UtcNow.AddHours(-3),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductHookahTropicalId,
                            ProductNameSnapshot = SharedSeedData.ProductHookahTropicalName,
                            UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalPrice,
                            Quantity = 1,
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
                            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-3)
                        }
                    }
                },

                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = SharedSeedData.UserId,
                    Status = "Draft",
                    TotalAmount = 620.00m,
                    Notes = null,
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                    UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductFalafelSetId,
                            ProductNameSnapshot = SharedSeedData.ProductFalafelSetName,
                            UnitPriceSnapshot = SharedSeedData.ProductFalafelSetPrice,
                            Quantity = 2,
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                        },
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductKugelId,
                            ProductNameSnapshot = SharedSeedData.ProductKugelName,
                            UnitPriceSnapshot = SharedSeedData.ProductKugelPrice,
                            Quantity = 1,
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                        }
                    }
                },

                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = SharedSeedData.AdminId,
                    Status = "Completed",
                    TotalAmount = 720.00m,
                    Notes = "Test order for system check",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = SharedSeedData.ProductKugelId,
                            ProductNameSnapshot = SharedSeedData.ProductKugelName,
                            UnitPriceSnapshot = SharedSeedData.ProductKugelPrice,
                            Quantity = 6,
                            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                        }
                    }
                }
            };

            await dbContext.Orders.AddRangeAsync(orders);
            await dbContext.SaveChangesAsync();

            var totalItems = orders.Sum(o => o.Items.Count);
            logger.LogInformation(
                "OrderService: Successfully seeded {OrderCount} orders with {ItemCount} items.",
                orders.Count,
                totalItems);
        }
        else
        {
            logger.LogInformation("OrderService: Orders table already contains data. Seeding skipped.");
        }
    }
}