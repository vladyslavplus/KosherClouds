namespace KosherClouds.Contracts.Reviews
{
    public record ReviewUpdatedEvent
    {
        public Guid ReviewId { get; init; }
        public Guid ProductId { get; init; }
        public int OldRating { get; init; }
        public int NewRating { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
