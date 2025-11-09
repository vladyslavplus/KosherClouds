using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.Entities;
using KosherClouds.CartService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Redis;

namespace KosherClouds.CartService.Services
{
    public class CartService : ICartService
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<CartService> _logger;
        private const string CartPrefix = "cart:";
        private static readonly TimeSpan CartTtl = TimeSpan.FromMinutes(30);

        public CartService(
            IRedisCacheService cache,
            ILogger<CartService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private static string GetCartKey(Guid userId) => $"{CartPrefix}{userId}";

        public async Task<ShoppingCartDto> GetCartDetailsAsync(Guid userId)
        {
            var key = GetCartKey(userId);
            var cart = await _cache.GetDataAsync<ShoppingCart>(key) ?? new ShoppingCart(userId);

            return new ShoppingCartDto
            {
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new ShoppingCartItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
        }

        public async Task<ShoppingCartDto> AddOrUpdateItemAsync(Guid userId, CartItemAddDto dto)
        {
            var key = GetCartKey(userId);
            var cart = await _cache.GetDataAsync<ShoppingCart>(key) ?? new ShoppingCart(userId);

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (item != null)
            {
                item.Quantity += dto.Quantity;

                if (item.Quantity <= 0)
                {
                    cart.Items.Remove(item);
                    _logger.LogInformation("Removed product {ProductId} due to non-positive quantity for user {UserId}.", dto.ProductId, userId);
                }
                else
                {
                    _logger.LogInformation("Adjusted product {ProductId} by {Delta} for user {UserId}. New quantity: {Quantity}.",
                        dto.ProductId, dto.Quantity, userId, item.Quantity);
                }
            }
            else if (dto.Quantity > 0)
            {
                cart.Items.Add(new ShoppingCartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
                _logger.LogInformation("Added new product {ProductId} (x{Quantity}) to cart for user {UserId}.", dto.ProductId, dto.Quantity, userId);
            }

            await _cache.SetDataAsync(key, cart, CartTtl);
            return await GetCartDetailsAsync(userId);
        }

        public async Task RemoveItemAsync(Guid userId, Guid productId)
        {
            var key = GetCartKey(userId);
            var cart = await _cache.GetDataAsync<ShoppingCart>(key);

            if (cart == null) return;

            var removed = cart.Items.RemoveAll(i => i.ProductId == productId);
            if (removed > 0)
            {
                await _cache.SetDataAsync(key, cart, CartTtl);
                _logger.LogInformation("Removed product {ProductId} from cart for user {UserId}.", productId, userId);
            }
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var key = GetCartKey(userId);
            await _cache.RemoveDataAsync(key);
            _logger.LogInformation("Cleared cart for user {UserId}.", userId);
        }
    }
}