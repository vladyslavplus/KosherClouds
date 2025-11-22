using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.BookingService.Parameters
{
    public class BookingParameters : QueryStringParameters
    {
        public Guid? UserId { get; set; }
        public DateTime? MinBookingDate { get; set; }
        public DateTime? MaxBookingDate { get; set; }
        public string? SearchTerm { get; set; }
        public bool IsValidDateRange =>
            (MinBookingDate == null || MaxBookingDate == null) || MaxBookingDate > MinBookingDate;
    }
}
