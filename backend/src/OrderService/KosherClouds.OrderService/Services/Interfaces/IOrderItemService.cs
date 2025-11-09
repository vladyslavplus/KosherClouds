namespace KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.OrderItem;
public interface IOrderItemService
{
    Task<IEnumerable<OrderItemResponseDto>> GetItemsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderItemResponseDto?> GetOrderItemByIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task UpdateOrderItemQuantityAsync(
        Guid itemId,
        int newQuantity,
        CancellationToken cancellationToken = default);
}