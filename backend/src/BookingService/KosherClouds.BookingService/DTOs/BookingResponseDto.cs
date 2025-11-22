namespace KosherClouds.BookingService.DTOs
{
    public class BookingResponseDto
    {
        public Guid Id { get; set; }
        public DateTime BookingDateTime { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public string Zone { get; set; } = "MainHall";
        public string Status { get; set; } = "Pending";
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public Guid UserId { get; set; }
        public List<HookahBookingDto> Hookahs { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalGuests => Adults + Children;
        public bool HasHookahs => Hookahs != null && Hookahs.Any();
        public int HookahCount => Hookahs?.Count ?? 0;
        public bool IsUpcoming => BookingDateTime > DateTime.UtcNow && Status != "Cancelled";
        public bool IsPast => BookingDateTime < DateTime.UtcNow;
        public bool CanBeCancelled => Status == "Pending" || Status == "Confirmed";
        public bool CanBeModified => (Status == "Pending" || Status == "Confirmed") && IsUpcoming;
        public string DisplayBookingDate => BookingDateTime.ToString("dd MMM yyyy");
        public string DisplayBookingTime => BookingDateTime.ToString("HH:mm");
        public string DisplayBookingDateTime => BookingDateTime.ToString("dd MMM yyyy, HH:mm");
        public string DisplayGuestCount => $"{Adults} adults" + (Children > 0 ? $", {Children} children" : "");
    }
}