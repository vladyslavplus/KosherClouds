namespace KosherClouds.ProductService.DTOs.Hookah;

using System.Collections.Generic;
using KosherClouds.ProductService.Entities.Enums;
using System.Text.Json.Serialization;
    public class HookahDetailsDto
    {
        public string TobaccoFlavor { get; set; } = string.Empty;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HookahStrength Strength { get; set; }
        
        public string? BowlType { get; set; }
        public Dictionary<string, string> AdditionalParams { get; set; } = new Dictionary<string, string>();
    }