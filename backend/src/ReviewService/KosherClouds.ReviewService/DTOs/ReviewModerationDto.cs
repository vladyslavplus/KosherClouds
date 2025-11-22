using System.ComponentModel.DataAnnotations;

namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewModerationDto
    {
        [Required]
        public string Action { get; set; } = string.Empty; // "Hide" or "Publish"

        [MaxLength(500)]
        public string? ModerationNotes { get; set; }
    }
}
