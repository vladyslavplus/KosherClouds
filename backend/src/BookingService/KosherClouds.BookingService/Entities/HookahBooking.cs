using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KosherClouds.BookingService.Entities
{
    public class HookahBooking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ProductId { get; set; }

        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProductNameUk { get; set; }

        [Required, MaxLength(100)]
        public string TobaccoFlavor { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TobaccoFlavorUk { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HookahStrength Strength { get; set; } = HookahStrength.Medium;

        public int? ServeAfterMinutes { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
        public decimal PriceSnapshot { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}