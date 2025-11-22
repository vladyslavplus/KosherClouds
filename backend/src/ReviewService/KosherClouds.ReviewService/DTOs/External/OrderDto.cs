namespace KosherClouds.ReviewService.DTOs.External
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
