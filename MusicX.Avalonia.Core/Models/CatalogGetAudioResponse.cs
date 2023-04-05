namespace MusicX.Avalonia.Core.Models;

public record CatalogGetAudioResponse(Catalog Catalog);

public record CatalogGetSectionResponse(Section Section,
                                        ICollection<CatalogProfile> Profiles,
                                        ICollection<CatalogGroup> Groups,
                                        ICollection<object> Albums,
                                        ICollection<CatalogAudio> Audios,
                                        ICollection<CatalogRecommendedPlaylist> RecommendedPlaylists,
                                        ICollection<CatalogPlaylist> Playlists,
                                        ICollection<CatalogAudioFollowingsUpdateInfo> AudioFollowingsUpdateInfo,
                                        ICollection<CatalogBanner> CatalogBanners,
                                        ICollection<object> NavigationTabs,
                                        ICollection<object> ReactionSets);

public record CatalogAction(
    CatalogAction Action,
    string Title,
    int RefItemsCount,
    string RefLayoutName,
    string RefDataType,
    string SectionId,
    string BlockId
);

public record CatalogAction2(
    string Type,
    string Target,
    string Url,
    string ConsumeReason
);

public record CatalogAds(
    string ContentId,
    string Duration,
    string AccountAgeType,
    string Puid1,
    string Puid22
);

public record CatalogAlbum(
    int Id,
    string Title,
    int OwnerId,
    string AccessKey,
    CatalogThumb Thumb
);

public record CatalogThumb(
    int Width,
    int Height,
    string Photo34,
    string Photo68,
    string Photo135,
    string Photo270,
    string Photo300,
    string Photo600,
    string Photo1200
);

public record CatalogTopTitle(
    string Icon,
    string Text
);

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

public record CatalogAudioFollowingsUpdateInfo(
    string Title,
    string Id,
    IReadOnlyList<CatalogCover> Covers
);

public record CatalogBadge(
    string Type,
    string Text
);

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

public record CatalogButton(
    CatalogAction Action,
    string Title
);

public record CatalogBanner(
    int Id,
    CatalogClickAction ClickAction,
    IReadOnlyList<CatalogButton> Buttons,
    IReadOnlyList<CatalogImage> Images,
    string Text,
    string Title,
    string TrackCode,
    string ImageMode
);

public record CatalogClickAction(
    Action Action
);

public record CatalogCover(
    int Width,
    int Height,
    string Photo34,
    string Photo68,
    string Photo135,
    string Photo270,
    string Photo300,
    string Photo600,
    string Photo1200
);

public record CatalogGroup(
    int Id,
    int MemberStatus,
    int MembersCount,
    string Activity,
    int Trending,
    string Name,
    string ScreenName,
    int IsClosed,
    string Type,
    int IsAdmin,
    int IsMember,
    int IsAdvertiser,
    int Verified,
    string Photo50,
    string Photo100,
    string Photo200
);

public record CatalogImage(
    string Url,
    int Width,
    int Height
);

public record CatalogMainArtist(
    string Name,
    string Domain,
    string Id,
    bool IsFollowed,
    bool CanFollow
);

public record CatalogMeta(
    string View
);

public record CatalogPermissions(
    bool Play,
    bool Share,
    bool Edit,
    bool Follow,
    bool Delete,
    bool BoomDownload,
    bool SaveAsCopy
);

public record CatalogPhoto(
    int Width,
    int Height,
    string Photo34,
    string Photo68,
    string Photo135,
    string Photo270,
    string Photo300,
    string Photo600,
    string Photo1200
);

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

public record CatalogRecommendedPlaylist(
    int Id,
    int OwnerId,
    double Percentage,
    string PercentageTitle,
    IReadOnlyList<string> Audios,
    string Color
);

public record CatalogCountry(
    int Id,
    string Title
);

public record CatalogCity(
    int Id,
    string Title
);

public record CatalogCareer(
    int CityId,
    int CountryId,
    int GroupId,
    string Position
);

public record CatalogProfile(
    int Id,
    CatalogCity City,
    CatalogCountry Country,
    string Photo200,
    string Activity,
    int FollowersCount,
    ICollection<CatalogCareer> Career,
    int University,
    string UniversityName,
    int Faculty,
    string FacultyName,
    int Graduation,
    string ScreenName,
    string Photo50,
    string Photo100,
    int Verified,
    int Trending,
    int FriendStatus,
    string FirstName,
    string LastName,
    bool CanAccessClosed,
    bool IsClosed,
    string EducationForm,
    string EducationStatus
);

public record Catalog(string DefaultSection, ICollection<CatalogSection> Sections);

public record CatalogSection(string Id, string Title, string Url);

public record Section(string Id, string Title, string Url, ICollection<SectionBlock> Blocks, string? NextFrom);

public record SectionBlock(string Id,
                           string DataType,
                           SectionBlockLayout Layout,
                           ICollection<int> CatalogBannerIds,
                           ICollection<string> ListenEvents,
                           CatalogBadge Badge,
                           ICollection<string> AudiosIds,
                           ICollection<string> PlaylistsIds,
                           ICollection<CatalogAction> Actions,
                           string NextFrom,
                           string Url,
                           ICollection<string> AudioFollowingsUpdateInfoIds);

public record SectionBlockLayout(string Name, int? OwnerId, string? Title, bool? IsEditable);