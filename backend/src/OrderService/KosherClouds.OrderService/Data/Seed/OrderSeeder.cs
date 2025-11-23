using KosherClouds.Common.Seed;
using KosherClouds.OrderService.Entities;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Data.Seed
{
    public static class OrderSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            await context.Database.MigrateAsync();

            if (await context.Orders.AnyAsync())
            {
                return;
            }

            var createdAt = DateTimeOffset.UtcNow.AddDays(-10);

            var order1 = new Order
            {
                Id = SharedSeedData.Order1Id,
                UserId = SharedSeedData.UserId,
                Status = OrderStatus.Paid,
                TotalAmount = SharedSeedData.ProductKugelPrice * 2 + SharedSeedData.ProductFalafelSetPrice,
                Notes = "Please deliver to the front desk",
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            order1.Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductKugelId,
                    ProductNameSnapshot = SharedSeedData.ProductKugelName,
                    UnitPriceSnapshot = SharedSeedData.ProductKugelPrice,
                    Quantity = 2,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    ProductNameSnapshot = SharedSeedData.ProductFalafelSetName,
                    UnitPriceSnapshot = SharedSeedData.ProductFalafelSetPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                }
            };

            var order2 = new Order
            {
                Id = SharedSeedData.Order2Id,
                UserId = SharedSeedData.ManagerId,
                Status = OrderStatus.Paid,
                TotalAmount = SharedSeedData.ProductHookahTropicalPrice * 2,
                Notes = null,
                CreatedAt = createdAt.AddDays(2),
                UpdatedAt = createdAt.AddDays(2)
            };

            order2.Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahTropicalName,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalPrice,
                    Quantity = 2,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                }
            };

            var order3 = new Order
            {
                Id = SharedSeedData.Order3Id,
                UserId = SharedSeedData.UserId,
                Status = OrderStatus.Paid,
                TotalAmount = SharedSeedData.ProductFalafelSetPrice + SharedSeedData.ProductHookahTropicalPrice,
                Notes = "Thank you!",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            };

            order3.Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order3.Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    ProductNameSnapshot = SharedSeedData.ProductFalafelSetName,
                    UnitPriceSnapshot = SharedSeedData.ProductFalafelSetPrice,
                    Quantity = 1,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order3.Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahTropicalName,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalPrice,
                    Quantity = 1,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
                }
            };

            await context.Orders.AddRangeAsync(order1, order2, order3);
            await context.SaveChangesAsync();
        }
    }
}