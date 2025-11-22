namespace KosherClouds.Contracts.Bookings
{
    public class BookingUpdatedEvent
    {
        public Guid BookingId { get; set; }
        public string? Comment { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
