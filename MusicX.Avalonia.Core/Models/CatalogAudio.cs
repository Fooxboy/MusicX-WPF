namespace MusicX.Avalonia.Core.Models;

public record CatalogAudio(
    string Artist,
    int Id,
    int OwnerId,
    string Title,
    int Duration,
    string AccessKey,
    CatalogAds Ads,
    bool IsExplicit,
    bool IsFocusTrack,
    bool IsLicensed,
    string TrackCode,
    string Url,
    int Date,
    int GenreId,
    bool ShortVideosAllowed,
    bool StoriesAllowed,
    bool StoriesCoverAllowed,
    bool? HasLyrics,
    CatalogAlbum Album,
    IReadOnlyList<CatalogMainArtist> MainArtists,
    string Subtitle,
    int? NoSearch
);