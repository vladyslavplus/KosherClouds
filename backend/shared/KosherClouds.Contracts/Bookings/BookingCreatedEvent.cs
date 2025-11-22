namespace KosherClouds.Contracts.Bookings
{
    public class BookingCreatedEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public DateTime BookingDateTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
