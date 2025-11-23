using Microsoft.AspNetCore.Mvc;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.Parameters;
using KosherClouds.ServiceDefaults.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace KosherClouds.OrderService.Controllers
{
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
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderParameters parameters,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdminOrManager = User.IsAdminOrManager();
            if (!isAdminOrManager)
            {
                parameters.UserId = userId;
            }

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

            var userId = User.GetUserId();
            if (userId != order.UserId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
                return Forbid();

            return Ok(order);
        }

        [HttpPost("from-cart")]
        public async Task<IActionResult> CreateOrderFromCart(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var createdOrder = await _orderService.CreateOrderFromCartAsync(
                userId.Value, cancellationToken);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = createdOrder.Id },
                createdOrder);
        }

        [HttpPut("{id:guid}/confirm")]
        public async Task<ActionResult<OrderResponseDto>> ConfirmDraft(
            Guid id,
            [FromBody] OrderConfirmDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var confirmed = await _orderService.ConfirmOrderAsync(
                id, userId.Value, request, cancellationToken);

            return Ok(confirmed);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateOrder(
            Guid id,
            [FromBody] OrderUpdateDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
}