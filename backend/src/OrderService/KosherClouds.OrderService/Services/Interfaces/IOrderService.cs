namespace KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.Parameters; 
using KosherClouds.ServiceDefaults.Helpers; 
public interface IOrderService
{
   Task<PagedList<OrderResponseDto>> GetOrdersAsync(
        OrderParameters parameters,
        CancellationToken cancellationToken = default);

   Task<OrderResponseDto?> GetOrderByIdAsync(
       Guid orderId,
       CancellationToken cancellationToken = default);

   Task<OrderResponseDto> CreateOrderAsync(
       OrderCreateDto orderDto,
       CancellationToken cancellationToken = default);

   Task UpdateOrderAsync(
       Guid orderId,
       OrderUpdateDto orderDto,
       CancellationToken cancellationToken = default);

    Task DeleteOrderAsync(
       Guid orderId,
       CancellationToken cancellationToken = default);
}