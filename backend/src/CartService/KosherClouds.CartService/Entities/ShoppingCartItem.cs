namespace KosherClouds.CartService.Entities;

public class ShoppingCartItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}