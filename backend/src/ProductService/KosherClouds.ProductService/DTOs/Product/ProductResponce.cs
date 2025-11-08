namespace KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.ProductService.DTOs.Hookah;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string? SubCategory { get; set; }

    public List<string> Ingredients { get; set; } = new List<string>();
    public List<string> Allergens { get; set; } = new List<string>();
    public List<string> Photos { get; set; } = new List<string>();

    public bool IsAvailable { get; set; }
    public double Rating { get; set; }
    public long RatingCount { get; set; }

    public HookahDetailsDto? HookahDetails { get; set; }
}