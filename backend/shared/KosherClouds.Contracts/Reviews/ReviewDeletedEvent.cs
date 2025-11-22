namespace KosherClouds.Contracts.Reviews
{
    public record ReviewDeletedEvent
    {
        public Guid ReviewId { get; init; }
        public Guid ProductId { get; init; }
        public int Rating { get; init; }
        public DateTimeOffset DeletedAt { get; init; }
    }
}
