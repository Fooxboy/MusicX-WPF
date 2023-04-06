namespace MusicX.Avalonia.Core.Models;

public record CatalogAudioFollowingsUpdateInfo(
    string Title,
    string Id,
    IReadOnlyList<CatalogCover> Covers
);