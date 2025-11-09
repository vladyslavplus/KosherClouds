using Microsoft.AspNetCore.Mvc;
using KosherClouds.OrderService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace KosherClouds.OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet("by-order/{orderId:guid}")]
        public async Task<IActionResult> GetItemsByOrderId(Guid orderId, CancellationToken cancellationToken)
        {
            var items = await _orderItemService.GetItemsByOrderIdAsync(orderId, cancellationToken);

            if (!items.Any())
                return NotFound("No items found for this order.");

            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderItemById(Guid id, CancellationToken cancellationToken)
        {
            var item = await _orderItemService.GetOrderItemByIdAsync(id, cancellationToken);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPut("{id:guid}/quantity")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateOrderItemQuantity(Guid id, [FromQuery] int newQuantity, CancellationToken cancellationToken)
        {
            await _orderItemService.UpdateOrderItemQuantityAsync(id, newQuantity, cancellationToken);
            return NoContent();
        }
    }
}