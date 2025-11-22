using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KosherClouds.BookingService.Entities
{
    public class Booking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime BookingDateTime { get; set; }

        [Required]
        public int Adults { get; set; }

        [Required]
        public int Children { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingZone Zone { get; set; } = BookingZone.MainHall;

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<HookahBooking> Hookahs { get; set; } = new();
    }
}
