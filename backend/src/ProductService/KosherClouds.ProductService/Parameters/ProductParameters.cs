namespace KosherClouds.ProductService.Parameters;

using KosherClouds.ServiceDefaults.Helpers;

public class ProductParameters : QueryStringParameters
{
    public string? Name { get; set; }
    public string? NameUk { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? SubCategoryUk { get; set; }
    public string? SearchTerm { get; set; }

    public bool? IsAvailable { get; set; }
    public bool? IsVegetarian { get; set; }
    public bool? IsPromotional { get; set; }
    public bool? IsHookah { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
}