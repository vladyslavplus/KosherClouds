namespace KosherClouds.ProductService.Entities;

using KosherClouds.ProductService.Entities.Enums;
using System.Text.Json.Serialization;


public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } 
    
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public ProductCategory Category { get; set; }
    
    public string? SubCategory { get; set; } 

    public List<string> Ingredients { get; set; } = new List<string>();
    public List<string> Allergens { get; set; } = new List<string>();
    public List<string> Photos { get; set; } = new List<string>(); 

    public bool IsAvailable { get; set; } = true;
    public double Rating { get; set; } = 0.0;
    public long RatingCount { get; set; } = 0;

    public HookahDetails? HookahDetails { get; set; }
}

