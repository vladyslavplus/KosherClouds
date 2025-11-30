using System.ComponentModel.DataAnnotations;

namespace KosherClouds.ReviewService.Entities
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        public Guid? ProductId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public ReviewType ReviewType { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        [Required]
        public bool IsVerifiedPurchase { get; set; } = true;

        [Required]
        public ReviewStatus Status { get; set; } = ReviewStatus.Published;

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAt { get; set; }

        [MaxLength(500)]
        public string? ModerationNotes { get; set; }

        public Guid? ModeratedBy { get; set; }

        public DateTimeOffset? ModeratedAt { get; set; }
    }
}
