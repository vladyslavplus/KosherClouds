namespace KosherClouds.Contracts.Reviews
{
    public record ReviewCreatedEvent
    {
        public Guid ReviewId { get; init; }
        public Guid ProductId { get; init; }
        public Guid UserId { get; init; }
        public int Rating { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
