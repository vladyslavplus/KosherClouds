namespace KosherClouds.ProductService.DTOs.Products;

using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.Entities.Enums;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameUk { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? DescriptionUk { get; set; }

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool IsPromotional { get; set; }
    public ProductCategory Category { get; set; }
    public string? SubCategory { get; set; }
    public string? SubCategoryUk { get; set; }
    public bool IsVegetarian { get; set; }

    public List<string> Ingredients { get; set; } = new();
    public List<string> IngredientsUk { get; set; } = new();

    public List<string> Allergens { get; set; } = new();
    public List<string> AllergensUk { get; set; } = new();

    public List<string> Photos { get; set; } = new();
    public bool IsAvailable { get; set; }
    public double Rating { get; set; }
    public long RatingCount { get; set; }

    public HookahDetailsDto? HookahDetails { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}