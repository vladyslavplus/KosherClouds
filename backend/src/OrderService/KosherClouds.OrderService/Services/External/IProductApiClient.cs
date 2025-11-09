using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public interface IProductApiClient
    {
        Task<ProductInfoDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
