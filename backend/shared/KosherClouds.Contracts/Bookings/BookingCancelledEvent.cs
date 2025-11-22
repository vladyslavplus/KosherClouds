namespace KosherClouds.Contracts.Bookings
{
    public record BookingCancelledEvent
    {
        public Guid BookingId { get; init; }
        public Guid UserId { get; init; }
        public DateTime OriginalBookingDateTime { get; init; }
        public DateTime CancelledAt { get; init; }
    }
}
