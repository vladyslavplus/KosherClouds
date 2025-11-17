namespace KosherClouds.Contracts.Payments
{
    public class PaymentCompletedEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
