namespace KosherClouds.CartService.Services;

using AutoMapper;
using Microsoft.Extensions.Logging;
using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.Entities;
using KosherClouds.CartService.Repositories.Interfaces;
using KosherClouds.CartService.Services.Interfaces;

    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository repository, IMapper mapper, ILogger<CartService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ShoppingCartDto> GetCartDetailsAsync(Guid userId)
        {
            var cart = await _repository.GetCartByUserIdAsync(userId);
            return _mapper.Map<ShoppingCartDto>(cart);
        }

        public async Task<ShoppingCartDto> AddOrUpdateItemAsync(Guid userId, CartItemAddDto dto)
        {
            var cart = await _repository.GetCartByUserIdAsync(userId);
            
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            // Імітація отримання реплікованих даних
            var tempProductData = new { Name = $"Продукт #{dto.ProductId.ToString().Substring(0, 4)}", Price = 100.00m, IsAvailable = true };
            
            if (!tempProductData.IsAvailable)
            {
                _logger.LogWarning("Attempted to add unavailable product {ProductId}", dto.ProductId);
                throw new Exception("Product is currently unavailable."); 
            }

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _logger.LogInformation("Updated quantity for product {ProductId} in cart for user {UserId}", dto.ProductId, userId);
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    ProductName = tempProductData.Name,
                    UnitPrice = tempProductData.Price,
                    IsAvailable = tempProductData.IsAvailable
                };
                cart.Items.Add(newItem);
                _logger.LogInformation("Added new product {ProductId} to cart for user {UserId}", dto.ProductId, userId);
            }

            await _repository.SaveChangesAsync();
            return _mapper.Map<ShoppingCartDto>(cart);
        }

        public async Task RemoveItemAsync(Guid userId, Guid productId)
        {
            var cart = await _repository.GetCartByUserIdAsync(userId);
            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await _repository.SaveChangesAsync();
                _logger.LogInformation("Removed product {ProductId} from cart for user {UserId}", productId, userId);
            }
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await _repository.GetCartByUserIdAsync(userId);
            if (cart.Items.Any())
            {
                cart.Items.Clear();
                await _repository.SaveChangesAsync();
                _logger.LogInformation("Cleared cart for user {UserId}", userId);
            }
        }
        
    }