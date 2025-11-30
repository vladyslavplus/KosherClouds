using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.ReviewService.Parameters
{
    public class ReviewParameters : QueryStringParameters
    {
        public Guid? ProductId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? OrderId { get; set; }
        public string? ReviewType { get; set; }
        public string? Status { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTimeOffset? MinCreatedDate { get; set; }
        public DateTimeOffset? MaxCreatedDate { get; set; }
        public bool? VerifiedOnly { get; set; }
        public string? SearchTerm { get; set; }

        public bool IsValidRatingRange =>
            (MaxRating == null || MinRating == null) ||
            (MaxRating >= MinRating && MinRating >= 1 && MaxRating <= 5);

        public bool IsValidDateRange =>
            (MaxCreatedDate == null || MinCreatedDate == null) ||
            MaxCreatedDate > MinCreatedDate;
    }
}