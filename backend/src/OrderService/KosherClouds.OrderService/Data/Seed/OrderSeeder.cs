using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KosherClouds.OrderService.Data.Seed;

public static class OrderSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        if (!await dbContext.Orders.AnyAsync())
        {
            var userId1 = new Guid("A1000000-0000-0000-0000-000000000001");
            var userId2 = new Guid("A1000000-0000-0000-0000-000000000002");
            var productPizzaId = new Guid("B2000000-0000-0000-0000-000000000001");
            var productDrinkId = new Guid("B2000000-0000-0000-0000-000000000003");
            var productSaladId = new Guid("B2000000-0000-0000-0000-000000000002");

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                Status = "Completed",
                PaymentMethod = "Card",
                TotalAmount = 540.00m,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                Notes = "Pick up at 18:00",

                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productPizzaId,
                        ProductNameSnapshot = "Pepperoni Pizza",
                        UnitPriceSnapshot = 250.00m,
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productDrinkId,
                        ProductNameSnapshot = "Coca-Cola 0.33L",
                        UnitPriceSnapshot = 45.00m,
                        Quantity = 1
                    }
                }
            };

            order1.Payments.Add(new PaymentRecord
            {
                Id = Guid.NewGuid(),
                Amount = order1.TotalAmount,
                Status = "Success",
                PaymentMethod = "Card",
                TransactionId = $"TRX_{Guid.NewGuid().ToString().Substring(0, 8)}"
            });

            var order2 = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId2,
                Status = "Pending",
                PaymentMethod = "Cash",
                TotalAmount = 180.00m,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),

                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productSaladId,
                        ProductNameSnapshot = "Caesar Salad",
                        UnitPriceSnapshot = 180.00m,
                        Quantity = 1
                    }
                }
            };

            dbContext.Orders.Add(order1);
            dbContext.Orders.Add(order2);

            await dbContext.SaveChangesAsync();

            Console.WriteLine("Initial order data (without delivery) successfully added.");
        }
        else
        {
            Console.WriteLine("The Orders table already contains data. Seeding skipped.");
        }
    }
}