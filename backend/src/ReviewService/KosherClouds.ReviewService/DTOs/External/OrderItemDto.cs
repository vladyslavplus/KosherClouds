namespace KosherClouds.ReviewService.DTOs.External
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public string? ProductNameSnapshotUk { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
        public int Quantity { get; set; }
    }
}