namespace KosherClouds.OrderService.DTOs.OrderItem
{
    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public string? ProductNameSnapshotUk { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Quantity * UnitPriceSnapshot;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}