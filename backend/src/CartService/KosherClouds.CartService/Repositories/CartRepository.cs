namespace KosherClouds.CartService.Repositories;

using Microsoft.EntityFrameworkCore;
using CartService.Entities;
using CartService.Repositories.Interfaces;
using CartService.Data; 

    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items) 
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await _context.ShoppingCarts.AddAsync(cart);
            }

            return cart;
        }

        public void DeleteCart(ShoppingCart cart)
        {
            _context.ShoppingCarts.Remove(cart);
        }
        
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task UpdateProductPriceAsync(Guid productId, decimal newPrice)
        {
            var itemsToUpdate = await _context.ShoppingCartItems
                .Where(item => item.ProductId == productId)
                .ToListAsync();

            foreach (var item in itemsToUpdate)
            {
                item.UnitPrice = newPrice;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkProductAsUnavailableAsync(Guid productId)
        {
            var itemsToUpdate = await _context.ShoppingCartItems
                .Where(item => item.ProductId == productId)
                .ToListAsync();

            foreach (var item in itemsToUpdate)
            {
                item.IsAvailable = false;
            }

            await _context.SaveChangesAsync();
        }
    }