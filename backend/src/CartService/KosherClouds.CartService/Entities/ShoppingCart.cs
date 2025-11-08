namespace KosherClouds.CartService.Entities;

public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
}