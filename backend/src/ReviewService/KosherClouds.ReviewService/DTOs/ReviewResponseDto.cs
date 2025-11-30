using KosherClouds.ReviewService.Entities;

namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public ReviewType ReviewType { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductNameUk { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? ModerationNotes { get; set; }
        public Guid? ModeratedBy { get; set; }
        public DateTimeOffset? ModeratedAt { get; set; }
    }
}