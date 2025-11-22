using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KosherClouds.BookingService.Entities
{
    public class HookahBooking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string TobaccoFlavor { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HookahStrength Strength { get; set; } = HookahStrength.Medium;

        public int? ServeAfterMinutes { get; set; }
        public string? Notes { get; set; }

        [Required]
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;
    }
}
