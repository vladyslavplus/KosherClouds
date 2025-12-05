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
                ContactName = _faker.Name.FullName(),
                ContactPhone = _faker.Phone.PhoneNumber("+380#########"),
                ContactEmail = _faker.Internet.Email(),
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
            order.ContactName = string.Empty;
            order.ContactPhone = string.Empty;
            order.ContactEmail = string.Empty;
            return order;
        }

        public static Order CreatePaidOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = OrderStatus.Paid;
            return order;
        }

        public static Order CreateCompletedOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = OrderStatus.Completed;
            return order;
        }

        public static Order CreateCanceledOrder(Guid userId)
        {
            var order = CreateValidOrder(userId);
            order.Status = OrderStatus.Canceled;
            return order;
        }

        public static Order CreateOrderWithStatus(Guid userId, OrderStatus status)
        {
            var order = CreateValidOrder(userId);
            order.Status = status;
            return order;
        }

        public static Order CreateOrderWithAmount(decimal amount)
        {
            var order = CreateValidOrder();
            order.TotalAmount = amount;
            return order;
        }

        public static Order CreateOldOrder(int daysOld)
        {
            var order = CreateValidOrder();
            order.CreatedAt = DateTimeOffset.UtcNow.AddDays(-daysOld);
            order.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-daysOld);
            return order;
        }

        public static Order CreateOrderWithPaymentType(PaymentType paymentType)
        {
            var order = CreateValidOrder();
            order.PaymentType = paymentType;
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
                ProductNameSnapshotUk = null,
                UnitPriceSnapshot = decimal.Parse(_faker.Commerce.Price(10, 500)),
                Quantity = _faker.Random.Int(1, 5),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        public static OrderItem CreateOrderItemWithUkrainianName(Guid orderId)
        {
            var item = CreateValidOrderItem(orderId);
            item.ProductNameSnapshotUk = "Український продукт";
            return item;
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
                ContactName = _faker.Name.FullName(),
                ContactPhone = _faker.Phone.PhoneNumber("+380#########"),
                ContactEmail = _faker.Internet.Email(),
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
                ProductNameSnapshotUk = null,
                UnitPriceSnapshot = decimal.Parse(_faker.Commerce.Price(10, 500)),
                Quantity = _faker.Random.Int(1, 5)
            };
        }

        public static OrderItemCreateDto CreateOrderItemWithUkrainianFields()
        {
            return new OrderItemCreateDto
            {
                ProductId = Guid.NewGuid(),
                ProductNameSnapshot = "Test Product",
                ProductNameSnapshotUk = "Тестовий продукт",
                UnitPriceSnapshot = 100m,
                Quantity = 2
            };
        }

        public static OrderConfirmDto CreateValidOrderConfirmDto()
        {
            return new OrderConfirmDto
            {
                ContactName = _faker.Name.FullName(),
                ContactPhone = _faker.Phone.PhoneNumber("+380#########"),
                Notes = _faker.Lorem.Sentence(),
                PaymentType = PaymentType.OnPickup
            };
        }

        public static OrderConfirmDto CreateOrderConfirmDtoWithPaymentType(PaymentType paymentType)
        {
            var dto = CreateValidOrderConfirmDto();
            dto.PaymentType = paymentType;
            return dto;
        }

        public static OrderUpdateDto CreateValidOrderUpdateDto()
        {
            return new OrderUpdateDto
            {
                Status = OrderStatus.Completed,
                Notes = "Order completed"
            };
        }

        public static OrderUpdateDto CreateOrderUpdateDtoWithStatus(OrderStatus status)
        {
            return new OrderUpdateDto
            {
                Status = status,
                Notes = $"Order status changed to {status}"
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

        public static CartItemDto CreateSingleCartItem()
        {
            return new CartItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 1
            };
        }

        public static ProductInfoDto CreateProductInfo(Guid productId, bool isAvailable = true)
        {
            var price = decimal.Parse(_faker.Commerce.Price(10, 500));

            return new ProductInfoDto
            {
                Id = productId,
                Name = _faker.Commerce.ProductName(),
                NameUk = null,
                Price = price,
                DiscountPrice = null,
                IsAvailable = isAvailable
            };
        }

        public static ProductInfoDto CreateProductInfoWithDiscount(Guid productId, decimal price, decimal discountPrice, bool isAvailable = true)
        {
            return new ProductInfoDto
            {
                Id = productId,
                Name = _faker.Commerce.ProductName(),
                NameUk = null,
                Price = price,
                DiscountPrice = discountPrice,
                IsAvailable = isAvailable
            };
        }

        public static ProductInfoDto CreateProductInfoWithUkrainianName(Guid productId, bool isAvailable = true)
        {
            var product = CreateProductInfo(productId, isAvailable);
            product.NameUk = "Український продукт";
            return product;
        }

        public static List<ProductInfoDto> CreateProductInfoList(List<Guid> productIds, bool allAvailable = true)
        {
            return productIds.Select(id => CreateProductInfo(id, allAvailable)).ToList();
        }

        public static UserInfoDto CreateUserInfo(Guid userId)
        {
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();

            return new UserInfoDto
            {
                Id = userId,
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                PhoneNumber = _faker.Phone.PhoneNumber("+380#########"),
                FirstName = firstName,
                LastName = lastName
            };
        }

        public static UserInfoDto CreateUserInfoWithoutPhone(Guid userId)
        {
            var user = CreateUserInfo(userId);
            user.PhoneNumber = null;
            return user;
        }

        public static UserInfoDto CreateUserInfoWithoutEmail(Guid userId)
        {
            var user = CreateUserInfo(userId);
            user.Email = null;
            return user;
        }

        public static UserInfoDto CreateUserInfoWithoutFullName(Guid userId)
        {
            return new UserInfoDto
            {
                Id = userId,
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                PhoneNumber = _faker.Phone.PhoneNumber("+380#########"),
                FirstName = null,
                LastName = null
            };
        }
    }
}