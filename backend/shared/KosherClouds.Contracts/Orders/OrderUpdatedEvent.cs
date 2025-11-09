namespace KosherClouds.Contracts.Orders
{
    public class OrderUpdatedEvent
    {
        public Guid OrderId { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
