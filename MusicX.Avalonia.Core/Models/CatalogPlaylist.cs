using System.Text.Json.Serialization;
using MusicX.Avalonia.Core.Converters;

namespace MusicX.Avalonia.Core.Models;

public record CatalogPlaylist(
    int Id,
    long OwnerId,
    int Type,
    string Title,
    string Description,
    int Count,
    int Followers,
    int Plays,
    [property: JsonConverter(typeof(DateTimeConverterForCustomStandardFormatR))] DateTime CreateTime,
    [property: JsonConverter(typeof(DateTimeConverterForCustomStandardFormatR))] DateTime UpdateTime,
    IReadOnlyList<object> Genres,
    bool IsFollowing,
    CatalogPhoto Photo,
    CatalogPermissions Permissions,
    bool SubtitleBadge,
    bool PlayButton,
    string AccessKey,
    string Subtitle,
    string AlbumType,
    CatalogMeta Meta,
    string? OwnerName,
    IReadOnlyList<CatalogMainArtist>? MainArtists);