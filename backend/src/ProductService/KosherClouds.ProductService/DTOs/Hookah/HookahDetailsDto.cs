namespace KosherClouds.ProductService.DTOs.Hookah;

using KosherClouds.ProductService.Entities.Enums;

public class HookahDetailsDto
{
    public string TobaccoFlavor { get; set; } = string.Empty;
    public HookahStrength Strength { get; set; }
    public string? BowlType { get; set; }
    public Dictionary<string, string> AdditionalParams { get; set; } = new();
}