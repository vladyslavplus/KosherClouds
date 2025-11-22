namespace KosherClouds.Contracts.Reviews
{
    public record ReviewStatusChangedEvent
    {
        public Guid ReviewId { get; init; }
        public Guid ProductId { get; init; }
        public int Rating { get; init; }
        public string OldStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTimeOffset ChangedAt { get; init; }
    }
}
