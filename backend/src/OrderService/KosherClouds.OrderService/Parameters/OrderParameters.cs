using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.OrderService.Parameters
{
    public class OrderParameters : QueryStringParameters
    {
        public Guid? UserId { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }

        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }

        public DateTimeOffset? MinOrderDate { get; set; }
        public DateTimeOffset? MaxOrderDate { get; set; }

        public bool IsValidDateRange => (MaxOrderDate == null || MinOrderDate == null) || MaxOrderDate > MinOrderDate;
    }
}