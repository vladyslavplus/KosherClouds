using KosherClouds.BookingService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.BookingService.DTOs
{
    public class HookahBookingDto
    {
        public Guid? ProductId { get; set; } // (filled from frontend)

        [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string? ProductName { get; set; }

        [MaxLength(100, ErrorMessage = "Product name (Ukrainian) cannot exceed 100 characters")]
        public string? ProductNameUk { get; set; }

        [Required(ErrorMessage = "Tobacco flavor is required")]
        [MaxLength(100, ErrorMessage = "Tobacco flavor name cannot exceed 100 characters")]
        public string TobaccoFlavor { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Tobacco flavor (Ukrainian) cannot exceed 100 characters")]
        public string? TobaccoFlavorUk { get; set; }

        [Required]
        [EnumDataType(typeof(HookahStrength), ErrorMessage = "Invalid strength. Must be: Light, Medium, or Strong")]
        public string Strength { get; set; } = "Medium";

        [Range(0, 240, ErrorMessage = "Serve time must be between 0 and 240 minutes (4 hours)")]
        public int? ServeAfterMinutes { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999999.99")]
        public decimal? PriceSnapshot { get; set; }
    }
}