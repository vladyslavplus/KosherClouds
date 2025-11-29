namespace KosherClouds.Contracts.Orders
{
    public class OrderItemInfo
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductNameUk { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
