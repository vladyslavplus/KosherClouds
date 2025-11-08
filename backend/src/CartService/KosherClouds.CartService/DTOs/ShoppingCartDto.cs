namespace KosherClouds.CartService.DTOs;


    public class ShoppingCartDto
    {
        public Guid UserId { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal GrandTotal => Items.Sum(item => item.TotalPrice);
        public bool HasUnavailableItems => Items.Any(item => !item.IsAvailable);
    }