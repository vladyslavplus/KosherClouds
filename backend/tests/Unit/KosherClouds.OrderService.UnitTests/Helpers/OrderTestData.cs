using Bogus;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.DTOs.External;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.Parameters;

namespace KosherClouds.OrderService.UnitTests.Helpers
{
    public static class OrderTestData
    {
        private static readonly Faker _faker = new Faker();

        public static Order CreateValidOrder(Guid? userId = null)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Status = OrderStatus.Pending,
                TotalAmount = 250.00m,
                Notes = _faker.Lorem.Sentence(),
                PaymentType = PaymentType.OnPickup,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            order.Items = new List<OrderItem>
            {
                CreateValidOrderItem(order.Id),
                CreateValidOrderItem(order.Id)
            };

            return order;
        }

        public static Order CreateDraftOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = OrderStatus.Draft;
            return order;
        }

        public static Order CreatePaidOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = OrderStatus.Paid;
            return order;
        }

        public static OrderItem CreateValidOrderItem(Guid orderId)
        {
            return new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = Guid.NewGuid(),
                ProductNameSnapshot = _faker.Commerce.ProductName(),
                UnitPriceSnapshot = decimal.Parse(_faker.Commerce.Price(10, 500)),
                Quantity = _faker.Random.Int(1, 5),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        public static List<Order> CreateOrderList(int count, Guid? userId = null)
        {
            var orders = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                orders.Add(CreateValidOrder(userId));
            }
            return orders;
        }

        public static OrderCreateDto CreateValidOrderCreateDto(Guid? userId = null)
        {
            return new OrderCreateDto
            {
                UserId = userId ?? Guid.NewGuid(),
                Notes = _faker.Lorem.Sentence(),
                PaymentType = PaymentType.OnPickup,
                Items = new List<OrderItemCreateDto>
                {
                    CreateValidOrderItemCreateDto(),
                    CreateValidOrderItemCreateDto()
                }
            };
        }

        public static OrderItemCreateDto CreateValidOrderItemCreateDto()
        {
            return new OrderItemCreateDto
            {
                ProductId = Guid.NewGuid(),
                ProductNameSnapshot = _faker.Commerce.ProductName(),
                UnitPriceSnapshot = decimal.Parse(_faker.Commerce.Price(10, 500)),
                Quantity = _faker.Random.Int(1, 5)
            };
        }

        public static OrderConfirmDto CreateValidOrderConfirmDto()
        {
            return new OrderConfirmDto
            {
                Notes = _faker.Lorem.Sentence(),
                PaymentType = PaymentType.OnPickup
            };
        }

        public static OrderUpdateDto CreateValidOrderUpdateDto()
        {
            return new OrderUpdateDto
            {
                Status = OrderStatus.Completed,
                Notes = "Order completed"
            };
        }

        public static OrderParameters CreateOrderParameters()
        {
            return new OrderParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
        }

        public static List<CartItemDto> CreateCartItems()
        {
            return new List<CartItemDto>
            {
                new CartItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 2
                },
                new CartItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            };
        }

        public static ProductInfoDto CreateProductInfo(Guid productId, bool isAvailable = true)
        {
            return new ProductInfoDto
            {
                Id = productId,
                Name = _faker.Commerce.ProductName(),
                Price = decimal.Parse(_faker.Commerce.Price(10, 500)),
                IsAvailable = isAvailable
            };
        }

        public static List<ProductInfoDto> CreateProductInfoList(List<Guid> productIds, bool allAvailable = true)
        {
            return productIds.Select(id => CreateProductInfo(id, allAvailable)).ToList();
        }
    }
}