namespace KosherClouds.CartService.Repositories.Interfaces;

using CartService.Entities;


public interface ICartRepository
{
    Task<ShoppingCart> GetCartByUserIdAsync(Guid userId);
    Task<bool> SaveChangesAsync();
    void DeleteCart(ShoppingCart cart);
    Task UpdateProductPriceAsync(Guid productId, decimal newPrice);
    Task MarkProductAsUnavailableAsync(Guid productId);
}