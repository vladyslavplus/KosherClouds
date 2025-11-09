namespace KosherClouds.Contracts.Cart
{
    public class CartCheckedOutEvent
    {
        public Guid UserId { get; set; }
        public DateTime CheckedOutAt { get; set; } = DateTime.UtcNow;
        public List<CartItem> Items { get; set; } = new();

        public class CartItem
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
