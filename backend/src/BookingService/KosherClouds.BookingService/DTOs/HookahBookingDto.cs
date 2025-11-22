using KosherClouds.BookingService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.BookingService.DTOs
{
    public class HookahBookingDto
    {
        [Required(ErrorMessage = "Tobacco flavor is required")]
        [MaxLength(100, ErrorMessage = "Tobacco flavor name cannot exceed 100 characters")]
        public string TobaccoFlavor { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(HookahStrength), ErrorMessage = "Invalid strength. Must be: Light, Medium, or Strong")]
        public string Strength { get; set; } = "Medium";

        [Range(0, 240, ErrorMessage = "Serve time must be between 0 and 240 minutes (4 hours)")]
        public int? ServeAfterMinutes { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
