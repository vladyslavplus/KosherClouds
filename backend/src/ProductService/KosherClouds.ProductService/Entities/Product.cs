namespace KosherClouds.ProductService.Entities;

using KosherClouds.ProductService.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameUk { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? DescriptionUk { get; set; }

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [Precision(10, 2)]
    public decimal? DiscountPrice { get; set; }

    public bool IsPromotional { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductCategory Category { get; set; }

    public string? SubCategory { get; set; }

    public string? SubCategoryUk { get; set; }

    public bool IsVegetarian { get; set; } = false;

    public List<string> Ingredients { get; set; } = new();
    public List<string> IngredientsUk { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
    public List<string> AllergensUk { get; set; } = new();
    public List<string> Photos { get; set; } = new();

    public bool IsAvailable { get; set; } = true;

    public double Rating { get; set; } = 0.0;
    public long RatingCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public HookahDetails? HookahDetails { get; set; }
}