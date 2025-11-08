namespace KosherClouds.CartService.Services.Interfaces;
using KosherClouds.CartService.DTOs;
public interface ICartService
{
    Task<ShoppingCartDto> GetCartDetailsAsync(Guid userId);
    Task<ShoppingCartDto> AddOrUpdateItemAsync(Guid userId, CartItemAddDto dto);
    Task RemoveItemAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
}