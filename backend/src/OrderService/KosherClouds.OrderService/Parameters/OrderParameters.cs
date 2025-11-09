namespace KosherClouds.OrderService.Parameters;

using KosherClouds.ServiceDefaults.Helpers;

    public class OrderParameters : QueryStringParameters
    {
     
        public string? Status { get; set; }
        public DateTimeOffset? MinOrderDate { get; set; }
        public DateTimeOffset? MaxOrderDate { get; set; }
        public bool IsValidDateRange => (MaxOrderDate == null || MinOrderDate == null) || MaxOrderDate > MinOrderDate;
        public string? OrderBy { get; set; }
    }