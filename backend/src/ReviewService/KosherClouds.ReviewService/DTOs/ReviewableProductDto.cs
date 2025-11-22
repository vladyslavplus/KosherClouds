namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewableProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public bool HasReview { get; set; }
        public Guid? ExistingReviewId { get; set; }
    }
}
