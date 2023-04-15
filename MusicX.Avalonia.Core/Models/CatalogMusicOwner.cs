namespace MusicX.Avalonia.Core.Models;

public record CatalogMusicOwner(string Id, ICollection<CatalogImage> Image, string Title, string Subtitle, string Url);