namespace KosherClouds.CartService.Entities;

public class ShoppingCart
{
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();

    public ShoppingCart(Guid userId)
    {
        UserId = userId;
    }
}