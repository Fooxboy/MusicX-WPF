namespace MusicX.Avalonia.Core.Models;

public record CatalogPlaylist(
    int Id,
    int OwnerId,
    int Type,
    string Title,
    string Description,
    int Count,
    int Followers,
    int Plays,
    int CreateTime,
    int UpdateTime,
    IReadOnlyList<object> Genres,
    bool IsFollowing,
    CatalogPhoto Photo,
    CatalogPermissions Permissions,
    bool SubtitleBadge,
    bool PlayButton,
    string AccessKey,
    string Subtitle,
    string AlbumType,
    CatalogMeta Meta
);