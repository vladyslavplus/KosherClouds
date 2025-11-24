using Bogus;
using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.Entities;

namespace KosherClouds.CartService.UnitTests.Helpers
{
    public static class CartTestData
    {
        private static readonly Faker _faker = new Faker();

        public static Guid CreateUserId() => Guid.NewGuid();

        public static ShoppingCart CreateEmptyCart(Guid userId)
        {
            return new ShoppingCart(userId);
        }

        public static ShoppingCart CreateCartWithItems(Guid userId, int itemCount = 3)
        {
            var cart = new ShoppingCart(userId);

            for (int i = 0; i < itemCount; i++)
            {
                cart.Items.Add(CreateCartItem());
            }

            return cart;
        }

        public static ShoppingCartItem CreateCartItem(Guid? productId = null, int? quantity = null)
        {
            return new ShoppingCartItem
            {
                ProductId = productId ?? Guid.NewGuid(),
                Quantity = quantity ?? _faker.Random.Int(1, 10)
            };
        }

        public static CartItemAddDto CreateCartItemAddDto(Guid? productId = null, int? quantity = null)
        {
            return new CartItemAddDto
            {
                ProductId = productId ?? Guid.NewGuid(),
                Quantity = quantity ?? _faker.Random.Int(1, 10)
            };
        }

        public static ShoppingCartDto CreateCartDto(Guid userId, int itemCount = 0)
        {
            var dto = new ShoppingCartDto
            {
                UserId = userId
            };

            for (int i = 0; i < itemCount; i++)
            {
                dto.Items.Add(new ShoppingCartItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = _faker.Random.Int(1, 10)
                });
            }

            return dto;
        }

        public static List<ShoppingCartItem> CreateCartItemList(int count)
        {
            var items = new List<ShoppingCartItem>();
            for (int i = 0; i < count; i++)
            {
                items.Add(CreateCartItem());
            }
            return items;
        }
    }
}
