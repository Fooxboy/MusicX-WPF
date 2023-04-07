namespace MusicX.Avalonia.Core.Models;

public record CatalogAlbum(
    int Id,
    string Title,
    int OwnerId,
    string AccessKey,
    CatalogPhoto Thumb
);