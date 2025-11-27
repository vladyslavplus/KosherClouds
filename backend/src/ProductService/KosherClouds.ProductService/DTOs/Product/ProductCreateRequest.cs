namespace KosherClouds.ProductService.DTOs.Products;

using System.ComponentModel.DataAnnotations;
using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.Entities.Enums;

public class ProductCreateRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameUk { get; set; }

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? DescriptionUk { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Price { get; set; }

    [Range(0.01, 9999.99)]
    public decimal? DiscountPrice { get; set; }

    public bool IsPromotional { get; set; }

    [Required]
    public ProductCategory Category { get; set; }

    public string? SubCategory { get; set; }
    public string? SubCategoryUk { get; set; }

    public bool IsVegetarian { get; set; }

    public List<string> Ingredients { get; set; } = new();
    public List<string> IngredientsUk { get; set; } = new();

    public List<string> Allergens { get; set; } = new();
    public List<string> AllergensUk { get; set; } = new();

    public List<string> Photos { get; set; } = new();

    public bool IsAvailable { get; set; } = true;

    public HookahDetailsDto? HookahDetails { get; set; }
}
