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

            // ============================================
            // ORDER 1 - User (Jane Smith) - 12 Products
            // ============================================
            var order1 = new Order
            {
                Id = SharedSeedData.Order1Id,
                UserId = SharedSeedData.UserId,
                Status = OrderStatus.Paid,
                PaymentType = PaymentType.OnPickup,
                TotalAmount = 2200.00m,
                ContactName = $"{SharedSeedData.UserFirstName} {SharedSeedData.UserLastName}",
                ContactPhone = "+380501234567",
                ContactEmail = SharedSeedData.UserEmail,
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
                    ProductNameSnapshotUk = SharedSeedData.ProductKugelNameUk,
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
                    ProductNameSnapshotUk = SharedSeedData.ProductFalafelSetNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductFalafelSetDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahTropicalName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHookahTropicalNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    ProductNameSnapshot = SharedSeedData.ProductHummusName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHummusNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHummusPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    ProductNameSnapshot = SharedSeedData.ProductShakshukaName,
                    ProductNameSnapshotUk = SharedSeedData.ProductShakshukaNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductShakshukaDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    ProductNameSnapshot = SharedSeedData.ProductChallengeName,
                    ProductNameSnapshotUk = SharedSeedData.ProductChallengeNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductChallengePrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductMatzoSoupId,
                    ProductNameSnapshot = SharedSeedData.ProductMatzoSoupName,
                    ProductNameSnapshotUk = SharedSeedData.ProductMatzoSoupNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductMatzoSoupPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    ProductNameSnapshot = SharedSeedData.ProductSchnitzelName,
                    ProductNameSnapshotUk = SharedSeedData.ProductSchnitzelNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductSchnitzelDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductBabkaId,
                    ProductNameSnapshot = SharedSeedData.ProductBabkaName,
                    ProductNameSnapshotUk = SharedSeedData.ProductBabkaNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductBabkaDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahBerryName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHookahBerryNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahBerryPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductLemonadeId,
                    ProductNameSnapshot = SharedSeedData.ProductLemonadeName,
                    ProductNameSnapshotUk = SharedSeedData.ProductLemonadeNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductLemonadePrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1.Id,
                    ProductId = SharedSeedData.ProductMintTeaId,
                    ProductNameSnapshot = SharedSeedData.ProductMintTeaName,
                    ProductNameSnapshotUk = SharedSeedData.ProductMintTeaNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductMintTeaPrice,
                    Quantity = 1,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                }
            };

            // ============================================
            // ORDER 2 - Manager (John Doe) - 10 Products
            // ============================================
            var order2 = new Order
            {
                Id = SharedSeedData.Order2Id,
                UserId = SharedSeedData.ManagerId,
                Status = OrderStatus.Paid,
                PaymentType = PaymentType.Online,
                TotalAmount = 1900.00m,
                ContactName = $"{SharedSeedData.ManagerFirstName} {SharedSeedData.ManagerLastName}",
                ContactPhone = "+380509876543",
                ContactEmail = SharedSeedData.ManagerEmail,
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
                    ProductId = SharedSeedData.ProductKugelId,
                    ProductNameSnapshot = SharedSeedData.ProductKugelName,
                    ProductNameSnapshotUk = SharedSeedData.ProductKugelNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductKugelPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductFalafelSetId,
                    ProductNameSnapshot = SharedSeedData.ProductFalafelSetName,
                    ProductNameSnapshotUk = SharedSeedData.ProductFalafelSetNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductFalafelSetDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductHookahTropicalId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahTropicalName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHookahTropicalNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductBrisketId,
                    ProductNameSnapshot = SharedSeedData.ProductBrisketName,
                    ProductNameSnapshotUk = SharedSeedData.ProductBrisketNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductBrisketDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductHummusId,
                    ProductNameSnapshot = SharedSeedData.ProductHummusName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHummusNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHummusPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductShakshukaId,
                    ProductNameSnapshot = SharedSeedData.ProductShakshukaName,
                    ProductNameSnapshotUk = SharedSeedData.ProductShakshukaNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductShakshukaDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductChallengeId,
                    ProductNameSnapshot = SharedSeedData.ProductChallengeName,
                    ProductNameSnapshotUk = SharedSeedData.ProductChallengeNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductChallengePrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductSchnitzelId,
                    ProductNameSnapshot = SharedSeedData.ProductSchnitzelName,
                    ProductNameSnapshotUk = SharedSeedData.ProductSchnitzelNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductSchnitzelDiscountPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductHookahBerryId,
                    ProductNameSnapshot = SharedSeedData.ProductHookahBerryName,
                    ProductNameSnapshotUk = SharedSeedData.ProductHookahBerryNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahBerryPrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2.Id,
                    ProductId = SharedSeedData.ProductPomegranatJuiceId,
                    ProductNameSnapshot = SharedSeedData.ProductPomegranatJuiceName,
                    ProductNameSnapshotUk = SharedSeedData.ProductPomegranatJuiceNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductPomegranatJuicePrice,
                    Quantity = 1,
                    CreatedAt = createdAt.AddDays(2),
                    UpdatedAt = createdAt.AddDays(2)
                }
            };

            // ============================================
            // ORDER 3 - User (Jane Smith) - 2 Products
            // ============================================
            var order3 = new Order
            {
                Id = SharedSeedData.Order3Id,
                UserId = SharedSeedData.UserId,
                Status = OrderStatus.Paid,
                PaymentType = PaymentType.OnPickup,
                TotalAmount = SharedSeedData.ProductFalafelSetDiscountPrice + SharedSeedData.ProductHookahTropicalDiscountPrice,
                ContactName = "Jane S.",
                ContactPhone = "+380631111111",
                ContactEmail = SharedSeedData.UserEmail,
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
                    ProductNameSnapshotUk = SharedSeedData.ProductFalafelSetNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductFalafelSetDiscountPrice,
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
                    ProductNameSnapshotUk = SharedSeedData.ProductHookahTropicalNameUk,
                    UnitPriceSnapshot = SharedSeedData.ProductHookahTropicalDiscountPrice,
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