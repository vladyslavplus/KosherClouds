namespace KosherClouds.ProductService.Entities;

using KosherClouds.ProductService.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

[Owned]
public class HookahDetails
{
    public string TobaccoFlavor { get; set; } = string.Empty;
    public string? TobaccoFlavorUk { get; set; } 

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HookahStrength Strength { get; set; }

    public string? BowlType { get; set; }
    public string? BowlTypeUk { get; set; }

    public Dictionary<string, string> AdditionalParams { get; set; } = new();
    public Dictionary<string, string>? AdditionalParamsUk { get; set; }
}