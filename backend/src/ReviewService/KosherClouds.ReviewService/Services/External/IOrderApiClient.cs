using KosherClouds.ReviewService.DTOs.External;

namespace KosherClouds.ReviewService.Services.External
{
    public interface IOrderApiClient
    {
        Task<OrderDto?> GetOrderByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        Task<List<OrderDto>> GetPaidOrdersForUserAsync(
            Guid userId,
            int daysBack,
            CancellationToken cancellationToken = default);
    }
}
