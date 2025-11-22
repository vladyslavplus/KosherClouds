using KosherClouds.BookingService.Entities;
using KosherClouds.BookingService.Validation;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.BookingService.DTOs
{
    public class BookingCreateDto
    {
        [Required(ErrorMessage = "Booking date and time is required")]
        [FutureDate(ErrorMessage = "Booking date must be in the future")]
        public DateTime BookingDateTime { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Number of adults must be between 1 and 50")]
        public int Adults { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "Number of children must be between 0 and 50")]
        public int Children { get; set; }

        [Required]
        [EnumDataType(typeof(BookingZone), ErrorMessage = "Invalid zone. Must be: Terrace, MainHall, or VIP")]
        public string Zone { get; set; } = "MainHall";

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Phone number must be in valid international format")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        [MaxLength(5, ErrorMessage = "Cannot add more than 5 hookahs per booking")]
        public List<HookahBookingDto>? Hookahs { get; set; }
    }
}
