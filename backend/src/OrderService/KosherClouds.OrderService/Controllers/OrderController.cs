using Microsoft.AspNetCore.Mvc;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.Parameters;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using KosherClouds.ServiceDefaults.Helpers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderParameters parameters, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersAsync(parameters, cancellationToken);
        
        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
        {
            orders.TotalCount,
            orders.PageSize,
            orders.CurrentPage,
            orders.TotalPages,
            orders.HasNext,
            orders.HasPrevious
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        
        if (order == null)
            return NotFound();

        return Ok(order);
    }

   
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto request, CancellationToken cancellationToken)
    {
        var createdOrder = await _orderService.CreateOrderAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = createdOrder.Id },
            createdOrder
        );
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderUpdateDto request, CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteOrderAsync(id, cancellationToken);
        return NoContent();
    }
}