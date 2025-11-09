namespace KosherClouds.CartService.Controllers;

using Microsoft.AspNetCore.Mvc;
using  KosherClouds.CartService.DTOs;
using  KosherClouds.CartService.DTOs;
using  KosherClouds.CartService.Services.Interfaces;

    [ApiController]
    [Route("api/v1/users/{userId:guid}/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Отримує вміст кошика для вказаного користувача.
        /// GET api/v1/users/{userId}/cart
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCartDto>> GetCart(Guid userId)
        {
            // У реальному житті, тут ви перевіряли б, чи userId відповідає автентифікованому користувачу
            var cart = await _cartService.GetCartDetailsAsync(userId);
            
            // Якщо CartService не створює кошик автоматично, тут може бути логіка 404
            return Ok(cart);
        }

        /// <summary>
        /// Додає або оновлює товар у кошику.
        /// POST api/v1/users/{userId}/cart/items
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShoppingCartDto>> AddItem(Guid userId, [FromBody] CartItemAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedCart = await _cartService.AddOrUpdateItemAsync(userId, dto);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                // Залежно від типу помилки, можна повернути 404 (якщо продукт не існує) або 400
                return BadRequest(new { error = ex.Message }); 
            }
        }
        
        /// <summary>
        /// Видаляє вказаний товар із кошика.
        /// DELETE api/v1/users/{userId}/cart/items/{productId}
        /// </summary>
        [HttpDelete("items/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveItem(Guid userId, Guid productId)
        {
            await _cartService.RemoveItemAsync(userId, productId);
            
            // 204 No Content - успішно оброблено, але відповіді немає
            return NoContent(); 
        }

        /// <summary>
        /// Очищає весь вміст кошика.
        /// DELETE api/v1/users/{userId}/cart
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
        
    }