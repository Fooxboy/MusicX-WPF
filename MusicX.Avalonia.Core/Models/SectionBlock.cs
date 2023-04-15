namespace MusicX.Avalonia.Core.Models;

public record SectionBlock(string Id,
                           string DataType,
                           SectionBlockLayout Layout,
                           ICollection<int> CatalogBannerIds,
                           ICollection<string> ListenEvents,
                           CatalogBadge Badge,
                           ICollection<string> AudiosIds,
                           ICollection<string> PlaylistsIds,
                           ICollection<string> LinksIds,
                           ICollection<string> ArtistsIds,
                           ICollection<string> VideosIds,
                           ICollection<string> ArtistVideosIds,
                           ICollection<CatalogAction> Actions,
                           string NextFrom,
                           string Url,
                           ICollection<string> AudioFollowingsUpdateInfoIds);