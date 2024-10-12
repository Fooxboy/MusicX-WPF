using MusicX.Core.Models;
using MusicX.Core.Models.General;
using System.Linq;

namespace MusicX.Core.Helpers;

public interface IIdentifiable
{
    string Identifier { get; }

    public static void Process(ResponseData data)
    {
        if (data.Block != null)
            ProcessBlock(data.Block, data);

        if (data.Replacements?.ReplacementsModels is { Count: > 0 } replacementsModels)
            foreach (var block in replacementsModels.SelectMany(b => b.ToBlocks))
            {
                ProcessBlock(block, data);
            }

        if (data.Playlists is { Count: > 0 } playlists)
            foreach (var playlist in playlists)
            {
                FillPlaylistOwnerName(playlist, data);
            }

        if (data.Playlist != null)
            FillPlaylistOwnerName(data.Playlist, data);

        if (data.Section != null)
            ProcessSection(data.Section, data);
    }

    private static IEnumerable<T> IntersectById<T>(IEnumerable<T>? enumerable, IEnumerable<string> ids)
        where T : IIdentifiable => enumerable?.IntersectBy(ids, static b => b.Identifier) ?? [];

    private static IEnumerable<T> IntersectById<T>(IEnumerable<T>? enumerable, IEnumerable<long> ids)
        where T : IIdentifiable => enumerable?.IntersectBy(ids, static b => long.Parse(b.Identifier)) ?? [];

    private static IEnumerable<T> IntersectById<T>(IEnumerable<T>? enumerable, IEnumerable<int> ids)
        where T : IIdentifiable => enumerable?.IntersectBy(ids, static b => int.Parse(b.Identifier)) ?? [];

    private static IEnumerable<T> IntersectById<T, TId>(IEnumerable<T>? enumerable, IEnumerable<TId> ids)
        where T : IIdentifiable where TId : IIdentifiable =>
        enumerable?.IntersectBy(ids.Select(b => int.Parse(b.Identifier)), static b => int.Parse(b.Identifier)) ?? [];

    private static void ProcessBlock(Block block, ResponseData data)
    {
        block.Curators.AddRange(IntersectById(data.Curators, block.CuratorsIds));
        block.Groups.AddRange(IntersectById(data.Groups, block.GroupIds));
        block.Groups.AddRange(IntersectById(data.Groups, block.GroupsItemsIds));
        block.Playlists.AddRange(IntersectById(data.Playlists, block.PlaylistsIds));
        block.Artists.AddRange(IntersectById(data.Artists, block.ArtistsIds));
        block.Audios.AddRange(IntersectById(data.Audios, block.AudiosIds));
        block.Suggestions.AddRange(IntersectById(data.Suggestions, block.SuggestionsIds));
        block.Banners.AddRange(IntersectById(data.CatalogBanners, block.CatalogBannerIds));
        block.Links.AddRange(IntersectById(data.Links, block.LinksIds));
        block.Texts.AddRange(IntersectById(data.Texts, block.TextIds));
        block.Suggestions.AddRange(IntersectById(data.Suggestions, block.SuggestionsIds));
        block.PodcastSliderItems.AddRange(IntersectById(data.PodcastSliderItems, block.PodcastSliderItemsIds));
        block.PodcastEpisodes.AddRange(IntersectById(data.PodcastEpisodes, block.PodcastEpisodesIds));
        block.Longreads.AddRange(IntersectById(data.Longreads, block.LongreadsIds));
        block.Videos.AddRange(IntersectById(data.Videos, block.VideosIds));
        block.ArtistVideos.AddRange(IntersectById(data.ArtistVideos, block.ArtistVideosIds));
        block.Placeholders.AddRange(IntersectById(data.Placeholders, block.PlaceholdersIds));
        block.MusicOwners.AddRange(IntersectById(data.MusicOwners, block.MusicOwnerIds));
        block.FollowingsUpdateInfos.AddRange(IntersectById(data.FollowingsUpdateInfos, block.FollowingUpdateInfoIds));
        block.RecommendedPlaylists.AddRange(IntersectById(data.RecommendedPlaylists, block.PlaylistsIds));
        foreach (var recommendedPlaylist in block.RecommendedPlaylists)
        {
            recommendedPlaylist.Audios.AddRange(IntersectById(data.Audios, recommendedPlaylist.AudiosIds));
            recommendedPlaylist.Playlist = IntersectById(data.Playlists, new[] { ((IIdentifiable)recommendedPlaylist).Identifier }).FirstOrDefault()!;
        }
        foreach (var audio in block.Audios)
        {
            audio.ParentBlockId = block.Id;
        }
    }

    private static void ProcessSection(Section section, ResponseData data)
    {
        foreach (var block in section.Blocks)
        {
            ProcessBlock(block, data);
        }
        RemoveEmptySeparators(section);
        RemoveAds(section);
        MergeRefBlocks(section);
    }

    private static void RemoveEmptySeparators(Section section)
    {
        var snippetsBannerIndex = section.Blocks.FindIndex(b => b is { Layout.Name: "snippets_banner" });

        if (snippetsBannerIndex < 0)
            return;

        section.Blocks.RemoveAt(snippetsBannerIndex);
        section.Blocks.RemoveAt(snippetsBannerIndex); // excess separator
    }

    private static void RemoveAds(Section section)
    {
        section.Blocks.RemoveAll(block =>
            block is { DataType: "radiostations" } or
            { DataType: "audio_content_cards" } or { DataType: "empty" } or
            { Layout.Title: "Радиостанции" or "Эфиры" or "Популярные подкасты" } ||
            (
                block is { Banners.Count: > 0 } &&
                block.Banners.RemoveAll(banner => banner.ClickAction?.Action.Url.Contains("subscription") is true ||
                                                  banner.ClickAction?.Action.Url.Contains("combo") is true ||
                                                  banner.ClickAction?.Action.Url.Contains("narrative") is true ||
                                                  banner.ClickAction?.Action.Url
                                                      .Contains("https://vk.com/app") is true ||
                                                  banner.ClickAction?.Action.Url.Contains("https://vk.com/vk_music") is
                                                      true) > 0 &&
                block.Banners.Count == 0
            ) ||
            (
                block is { Links.Count: > 0 } &&
                block.Links.RemoveAll(link => link.Url.Contains("audio_offline") ||
                                              link.Url.Contains("radiostations") ||
                                              link.Url.Contains("music_transfer") ||
                                              link.Url.Contains("subscription") ||
                                              link.Url.Contains("audiobooks_favorites")) > 0 &&
                block.Links.Count == 0
            )
        );

        if (section.Blocks.FirstOrDefault() is { Layout.Name: "separator" })
            section.Blocks.RemoveAt(0);
    }

    private static void MergeRefBlocks(Section section)
    {
        for (var i = 0; i < section.Blocks.Count; i++)
        {
            var block = section.Blocks[i];

            if (block is not { DataType: "action", Layout.Name: "horizontal_buttons" })
                continue;

            var refBlockIndex = section.Blocks.FindIndex(b =>
                b.DataType == block.Actions[0].RefDataType && b.Layout?.Name == block.Actions[0].RefLayoutName) - 1;

            if (refBlockIndex < 0 || block.Actions[0].Action.Type != "open_section")
                continue;

            section.Blocks[refBlockIndex].Actions.AddRange(block.Actions);
            section.Blocks.RemoveAt(i);
            i--;
        }
    }

    private static void FillPlaylistOwnerName(Playlist playlist, ResponseData data)
    {
        var ownerId = playlist.Original?.OwnerId ?? playlist.OwnerId;

        if (ownerId < 0)
        {
            var value = data.Groups?.FirstOrDefault(b => b.Id == -ownerId);
            playlist.OwnerName = value?.Name;
        }
        else
        {
            var value = data.Profiles?.FirstOrDefault(p => p.Id == ownerId);
            playlist.OwnerName = $"{value?.FirstName} {value?.LastName}";
        }
    }
}