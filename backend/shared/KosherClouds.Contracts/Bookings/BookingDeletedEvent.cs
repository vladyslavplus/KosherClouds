namespace KosherClouds.Contracts.Bookings
{
    public class BookingDeletedEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    }
}
