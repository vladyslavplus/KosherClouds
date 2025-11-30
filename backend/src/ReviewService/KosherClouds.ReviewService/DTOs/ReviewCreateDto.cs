using KosherClouds.ReviewService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewCreateDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public ReviewType ReviewType { get; set; }

        public Guid? ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}