namespace KosherClouds.OrderService.Parameters;

using KosherClouds.ServiceDefaults.Helpers;

public class PaymentParameters : QueryStringParameters
{
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionSearchTerm { get; set; }
    public string? OrderBy { get; set; }
}