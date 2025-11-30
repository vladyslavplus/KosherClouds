namespace KosherClouds.ReviewService.DTOs
{
    public class OrderToReviewDto
    {
        public Guid OrderId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public bool OrderReviewExists { get; set; }
        public Guid? OrderReviewId { get; set; }
        public List<ReviewableProductDto> Products { get; set; } = new();
        public int ReviewableProductsCount { get; set; }
        public int DaysLeftToReview { get; set; }
    }
}