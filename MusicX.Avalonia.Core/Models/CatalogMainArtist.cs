namespace MusicX.Avalonia.Core.Models;

public record CatalogMainArtist(
    string Name,
    string Domain,
    string Id,
    bool IsFollowed,
    bool CanFollow,
    IReadOnlyList<CatalogImage> Photo);