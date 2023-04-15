using System.Text.Json.Serialization;
using MusicX.Avalonia.Core.Converters;

namespace MusicX.Avalonia.Core.Models;

public record CatalogVideo(IReadOnlyList<CatalogMainArtist> MainArtists, 
                           string Subtitle,
                           [property: JsonConverter(typeof(DateTimeConverterForCustomStandardFormatR))]DateTime ReleaseDate,
                           [property: JsonConverter(typeof(DateTimeConverterForCustomStandardFormatR))]DateTime Date,
                           string Description,
                           int Duration,
                           [property: JsonPropertyName("image")] IReadOnlyList<CatalogImage> Images,
                           int Width, int Height,
                           int Id,
                           long OwnerId,
                           long UserId,
                           string Title,
                           int Views,
                           string Player,
                           [property: JsonPropertyName("first_frame")] IReadOnlyList<CatalogVideoFirstFrame> FirstFrames);
                           
public record CatalogVideoFirstFrame(string Url, int Width, int Height);