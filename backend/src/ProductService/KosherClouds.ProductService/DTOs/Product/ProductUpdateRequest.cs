namespace KosherClouds.ProductService.DTOs.Products;

using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.Entities.Enums;

public class ProductUpdateRequest
{
    public string? Name { get; set; }
    public string? NameUk { get; set; }

    public string? Description { get; set; }
    public string? DescriptionUk { get; set; }

    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool? IsPromotional { get; set; }
    public ProductCategory? Category { get; set; }

    public string? SubCategory { get; set; }
    public string? SubCategoryUk { get; set; }

    public bool? IsVegetarian { get; set; }

    public List<string>? Ingredients { get; set; }
    public List<string>? IngredientsUk { get; set; }

    public List<string>? Allergens { get; set; }
    public List<string>? AllergensUk { get; set; }

    public List<string>? Photos { get; set; }
    public bool? IsAvailable { get; set; }

    public double? Rating { get; set; }
    public long? RatingCount { get; set; }

    public HookahDetailsDto? HookahDetails { get; set; }
}