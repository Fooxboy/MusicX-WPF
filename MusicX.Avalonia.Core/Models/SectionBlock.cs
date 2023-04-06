namespace MusicX.Avalonia.Core.Models;

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