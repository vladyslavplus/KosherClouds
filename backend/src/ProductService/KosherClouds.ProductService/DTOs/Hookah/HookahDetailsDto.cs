namespace KosherClouds.ProductService.DTOs.Hookah;

using KosherClouds.ProductService.Entities.Enums;

public class HookahDetailsDto
{
    public string TobaccoFlavor { get; set; } = string.Empty;
    public string? TobaccoFlavorUk { get; set; }
    public HookahStrength Strength { get; set; }
    public string? BowlType { get; set; }
    public string? BowlTypeUk { get; set; }
    public Dictionary<string, string> AdditionalParams { get; set; } = new();
    public Dictionary<string, string>? AdditionalParamsUk { get; set; }
}