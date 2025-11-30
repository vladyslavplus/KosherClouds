namespace KosherClouds.ReviewService.DTOs
{
    public class ReviewableProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductNameUk { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool AlreadyReviewed { get; set; }
        public Guid? ExistingReviewId { get; set; }
    }
}