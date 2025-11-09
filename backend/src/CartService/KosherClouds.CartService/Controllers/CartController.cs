using KosherClouds.CartService.DTOs;
using KosherClouds.CartService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.CartService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken token)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                _logger.LogWarning("User ID not found in JWT token.");
                return Unauthorized("User ID not found.");
            }

            var cart = await _cartService.GetCartDetailsAsync(userId.Value);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItemAddDto dto, CancellationToken token)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                _logger.LogWarning("User ID not found in JWT token.");
                return Unauthorized("User ID not found.");
            }

            var updated = await _cartService.AddOrUpdateItemAsync(userId.Value, dto);
            return Ok(updated);
        }

        [HttpDelete("items/{productId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken token)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                _logger.LogWarning("User ID not found in JWT token.");
                return Unauthorized("User ID not found.");
            }

            await _cartService.RemoveItemAsync(userId.Value, productId);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken token)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                _logger.LogWarning("User ID not found in JWT token.");
                return Unauthorized("User ID not found.");
            }

            await _cartService.ClearCartAsync(userId.Value);
            return NoContent();
        }
    }
}