namespace KosherClouds.Contracts.Orders
{
    public class OrderDeletedEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset DeletedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
