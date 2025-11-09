namespace KosherClouds.OrderService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;

public class OrderItemParameters : QueryStringParameters
{
    public Guid? ProductId { get; set; }
    public string? ProductNameSearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public string? OrderBy { get; set; }
}