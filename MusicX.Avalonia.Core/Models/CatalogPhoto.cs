using System.Text.Json.Serialization;

namespace MusicX.Avalonia.Core.Models;

public record CatalogPhoto(
    int Width,
    int Height,
    [property: JsonPropertyName("photo_34")] string Photo34,
    [property: JsonPropertyName("photo_68")] string Photo68,
    [property: JsonPropertyName("photo_135")] string Photo135,
    [property: JsonPropertyName("photo_270")] string Photo270,
    [property: JsonPropertyName("photo_300")] string Photo300,
    [property: JsonPropertyName("photo_600")] string Photo600,
    [property: JsonPropertyName("photo_1200")] string Photo1200
);