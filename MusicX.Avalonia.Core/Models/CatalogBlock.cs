namespace MusicX.Avalonia.Core.Models;

public record CatalogBlock(
    string Id,
    string DataType,
    SectionBlockLayout Layout,
    IReadOnlyList<int> CatalogBannerIds,
    IReadOnlyList<string> ListenEvents,
    CatalogBadge Badge,
    IReadOnlyList<string> AudiosIds,
    IReadOnlyList<string> PlaylistsIds,
    IReadOnlyList<CatalogAction> Actions,
    string NextFrom,
    string Url,
    IReadOnlyList<string> AudioFollowingsUpdateInfoIds
);