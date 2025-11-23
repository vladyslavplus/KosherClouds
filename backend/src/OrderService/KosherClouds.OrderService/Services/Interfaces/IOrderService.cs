using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.OrderService.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PagedList<OrderResponseDto>> GetOrdersAsync(
            OrderParameters parameters,
            CancellationToken cancellationToken = default);

        Task<OrderResponseDto?> GetOrderByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        Task<OrderResponseDto> CreateOrderFromCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<OrderResponseDto> CreateDraftOrderAsync(
            OrderCreateDto orderDto,
            CancellationToken cancellationToken = default);

        Task<OrderResponseDto> ConfirmOrderAsync(
            Guid orderId,
            Guid userId,
            OrderConfirmDto? request,
            CancellationToken cancellationToken = default);

        Task MarkOrderAsPaidAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        Task UpdateOrderAsync(
            Guid orderId,
            OrderUpdateDto orderDto,
            CancellationToken cancellationToken = default);

        Task DeleteOrderAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);
    }
}