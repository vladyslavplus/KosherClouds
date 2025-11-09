using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public interface ICartApiClient
    {
        Task<List<CartItemDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
        Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
