using System.ComponentModel.DataAnnotations;

namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewUpdateDto
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}
