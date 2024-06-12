using MusicX.Core.Models;
using MusicX.Core.Models.Abstractions;

namespace MusicX.Core.Helpers;

internal static partial class BlockExtensions
{
    private static IEnumerable<Suggestion> GetSuggestionsForBlock(this Block block, List<Suggestion> suggestions) =>
        GetItemsForBlock(block.SuggestionsIds, suggestions);

    private static IEnumerable<Placeholder> GetPlaceholdersForBlock(this Block block, List<Placeholder> placeholders) =>
        GetItemsForBlock(block.PlaceholdersIds, placeholders);

    private static IEnumerable<Artist> GetArtistsForBlock(this Block block, List<Artist> artists) =>
        GetItemsForBlock(block.ArtistsIds, artists);

    private static IEnumerable<Text> GetTextsForBlock(this Block block, List<Text> texts) =>
        GetItemsForBlock(block.TextIds, texts);

    private static IEnumerable<Group> GetGroupsForBlock(this Block block, List<Group> groups)
    {
        var groupIds = block.GroupIds.Concat(
            block.GroupsItemsIds.Select(g => (long)g.Id));

        return GetItemsForBlock(groupIds, groups);
    }

    private static IEnumerable<Curator> GetCuratorsForBlock(this Block block, List<Curator> curators) =>
        GetItemsForBlock(block.CuratorsIds, curators);

    private static IEnumerable<MusicOwner> GetMusicOwnersForBlock(this Block block, List<MusicOwner> musicOwners) =>
        GetItemsForBlock(block.MusicOwnerIds, musicOwners);


    private static IEnumerable<AudioFollowingsUpdateInfo> GetFollowingsUpdateInfosForBlock(
        this Block block,
        List<AudioFollowingsUpdateInfo> followingsUpdateInfos) =>
        GetItemsForBlock(block.FollowingUpdateInfoIds, followingsUpdateInfos);

    private static IEnumerable<CatalogBanner> GetCatalogBannersForBlock(
        this Block block,
        List<CatalogBanner> catalogBanners) =>
        GetItemsForBlock(block.CatalogBannerIds, catalogBanners);

    private static IEnumerable<Link> GetLinksForBlock(this Block block, List<Link> links) =>
        GetItemsForBlock(block.LinksIds, links);

    private static IEnumerable<Audio> GetAudiosForBlock(this Block block, List<Audio> audios) =>
        GetItemsWithOwnerIdForBlock<long, long, Audio>(block.AudiosIds, audios);

    private static IEnumerable<Playlist> GetPlaylistsForBlock(this Block block, List<Playlist> playlists) =>
        GetItemsWithOwnerIdForBlock<long, long, Playlist>(block.PlaylistsIds, playlists);

    private static IEnumerable<PodcastSliderItem> GetPodcastSliderItemsForBlock(
        this Block block,
        List<PodcastSliderItem> podcastSliderItems) =>
        GetItemsForBlock(block.PodcastSliderItemsIds, podcastSliderItems);

    private static IEnumerable<PodcastEpisode> GetPodcastEpisodesForBlock(
        this Block block,
        List<PodcastEpisode> podcastEpisodes) =>
        GetItemsWithOwnerIdForBlock<int, int, PodcastEpisode>(block.PodcastEpisodesIds, podcastEpisodes);

    private static IEnumerable<Longread> GetLongreadsForBlock(
        this Block block,
        List<Longread> longreads) =>
        GetItemsWithOwnerIdForBlock<int, int, Longread>(block.LongreadsIds, longreads);

    private static IEnumerable<Video> GetVideosForBlock(
        this Block block,
        List<Video> videos) =>
        GetItemsWithOwnerIdForBlock<int, int, Video>(block.VideosIds, videos);

    private static IEnumerable<Video> GetArtistVideosForBlock(
        this Block block,
        List<Video> artistVideos) =>
        GetItemsWithOwnerIdForBlock<int, int, Video>(block.ArtistVideosIds, artistVideos);

    private static IEnumerable<TItem> GetItemsForBlock<TId, TItem>(
        IEnumerable<TId> blockItemIds,
        List<TItem> responseItems)
        where TItem : class, IBlockEntity<TId>
    {
        var idComparer = EqualityComparer<TId>.Default;

        foreach (var itemId in blockItemIds)
        {
            var item = responseItems.Find(x => idComparer.Equals(x.Id, itemId));

            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<TItem> GetItemsWithOwnerIdForBlock<TId, TOwnerId, TItem>(
        IEnumerable<string> blockItemIds,
        List<TItem> responseItems)
        where TItem : class, IBlockEntityWithOwner<TId, TOwnerId>
        where TId : IParsable<TId>
        where TOwnerId : IParsable<TOwnerId>
    {
        var idComparer = EqualityComparer<TId>.Default;
        var ownerIdComparer = EqualityComparer<TOwnerId>.Default;

        foreach (var itemId in blockItemIds)
        {
            var idsArray = itemId.Split('_');
            var audioId = TId.Parse(idsArray[1], default);
            var ownerId = TOwnerId.Parse(idsArray[0], default);

            var item = responseItems.Find(x =>
                idComparer.Equals(x.Id, audioId) &&
                ownerIdComparer.Equals(x.OwnerId, ownerId));

            if (item != null)
            {
                // TODO: заменить этот костыль
                if (item is Audio audio)
                {
                    audio.ParentBlockId = itemId;
                }

                yield return item;
            }
        }
    }
}