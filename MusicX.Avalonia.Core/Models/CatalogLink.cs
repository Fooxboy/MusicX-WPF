using System.Text.Json.Serialization;

namespace MusicX.Avalonia.Core.Models;

public record CatalogLink(string Id, [property: JsonPropertyName("image")] IReadOnlyList<CatalogImage> Images,
                          CatalogLinkMeta Meta, string Subtitle, string Title, string Url);