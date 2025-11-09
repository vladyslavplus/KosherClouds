namespace KosherClouds.CartService.DTOs;
public class ShoppingCartDto
{
    public Guid UserId { get; set; }
    public List<ShoppingCartItemDto> Items { get; set; } = new();
}