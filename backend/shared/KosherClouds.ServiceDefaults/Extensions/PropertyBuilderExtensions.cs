using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace KosherClouds.ServiceDefaults.Extensions
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<List<string>> HasJsonConversion(this PropertyBuilder<List<string>> builder)
        {
            var comparer = new ValueComparer<List<string>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                c => c == null ? new List<string>() : new List<string>(c));

            builder.HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(comparer);

            return builder;
        }

        public static PropertyBuilder<Dictionary<string, string>> HasJsonConversion(this PropertyBuilder<Dictionary<string, string>> builder)
        {
            var comparer = new ValueComparer<Dictionary<string, string>>(
                (d1, d2) => JsonSerializer.Serialize(d1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(d2, (JsonSerializerOptions?)null),
                d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null).GetHashCode(),
                d => d == null ? new Dictionary<string, string>() : new Dictionary<string, string>(d));

            builder.HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                .Metadata.SetValueComparer(comparer);

            return builder;
        }
    }
}
