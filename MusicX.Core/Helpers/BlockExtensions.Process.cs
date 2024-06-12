using MusicX.Core.Models;

namespace MusicX.Core.Helpers;

internal static partial class BlockExtensions
{
    public static void ProcessAudios(this Block block, List<Audio> audios) =>
        block.Audios.AddRange(block.GetAudiosForBlock(audios));

    public static void ProcessPlaylists(this Block block, List<Playlist> playlists) =>
        block.Playlists.AddRange(block.GetPlaylistsForBlock(playlists));

    public static void ProcessCatalogBanners(this Block block, List<CatalogBanner> catalogBanners) =>
        block.Banners.AddRange(block.GetCatalogBannersForBlock(catalogBanners));

    public static void ProcessLinks(this Block block, List<Link> links) =>
        block.Links.AddRange(block.GetLinksForBlock(links));

    public static void ProcessSuggestions(this Block block, List<Suggestion> suggestions) =>
        block.Suggestions.AddRange(block.GetSuggestionsForBlock(suggestions));

    public static void ProcessPlaceholders(this Block block, List<Placeholder> placeholders) =>
        block.Placeholders.AddRange(block.GetPlaceholdersForBlock(placeholders));

    public static void ProcessArtists(this Block block, List<Artist> artists) =>
        block.Artists.AddRange(block.GetArtistsForBlock(artists));

    public static void ProcessTexts(this Block block, List<Text> texts) =>
        block.Texts.AddRange(block.GetTextsForBlock(texts));

    public static void ProcessGroups(this Block block, List<Group> groups) =>
        block.Groups.AddRange(block.GetGroupsForBlock(groups));

    public static void ProcessCurators(this Block block, List<Curator> curators) =>
        block.Curators.AddRange(block.GetCuratorsForBlock(curators));

    public static void ProcessMusicOwners(this Block block, List<MusicOwner> musicOwners) =>
        block.MusicOwners.AddRange(block.GetMusicOwnersForBlock(musicOwners));

    public static void ProcessPodcastSliderItems(this Block block, List<PodcastSliderItem> podcastSliderItems) =>
        block.PodcastSliderItems.AddRange(block.GetPodcastSliderItemsForBlock(podcastSliderItems));

    public static void ProcessPodcastEpisodes(this Block block, List<PodcastEpisode> podcastEpisodes) =>
        block.PodcastEpisodes.AddRange(block.GetPodcastEpisodesForBlock(podcastEpisodes));

    public static void ProcessLongreads(this Block block, List<Longread> longreads) =>
        block.Longreads.AddRange(block.GetLongreadsForBlock(longreads));

    public static void ProcessVideos(this Block block, List<Video> videos) =>
        block.Videos.AddRange(block.GetVideosForBlock(videos));

    public static void ProcessArtistVideos(this Block block, List<Video> artistVideos) =>
        block.ArtistVideos.AddRange(block.GetArtistVideosForBlock(artistVideos));

    public static void ProcessFollowingsUpdateInfos(
        this Block block,
        List<AudioFollowingsUpdateInfo> followingsUpdateInfos) =>
        block.FollowingsUpdateInfos.AddRange(block.GetFollowingsUpdateInfosForBlock(followingsUpdateInfos));
}