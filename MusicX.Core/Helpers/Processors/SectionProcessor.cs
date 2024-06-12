using MusicX.Core.Models;
using MusicX.Core.Models.General;

namespace MusicX.Core.Helpers.Processors;

internal static class SectionProcessor
{
    public static void Process(ResponseData response)
    {
        if (response.Section == null)
        {
            return;
        }

        foreach (var block in response.Section.Blocks)
        {
            block.ProcessAudios(response.Audios);
            block.ProcessCatalogBanners(response.CatalogBanners);
            block.ProcessLinks(response.Links);
            block.ProcessPlaceholders(response.Placeholders);
            block.ProcessSuggestions(response.Suggestions);
            block.ProcessArtists(response.Artists);
            block.ProcessTexts(response.Texts);
            block.ProcessGroups(response.Groups);
            block.ProcessPodcastSliderItems(response.PodcastSliderItems);
            block.ProcessPodcastEpisodes(response.PodcastEpisodes);
            block.ProcessLongreads(response.Longreads);
            block.ProcessVideos(response.Videos);
            block.ProcessArtistVideos(response.ArtistVideos);
            block.ProcessMusicOwners(response.MusicOwners);
            block.ProcessFollowingsUpdateInfos(response.FollowingsUpdateInfos);

            ProcessSectionPlaylists(block, response);
        }

        ProcessSnippets(response);
        RemoveGarbageBlocks(response);
        ProcessHorizontalButtons(response);
    }

    private static void ProcessSnippets(ResponseData response)
    {
        var snippetsBannerIndex = response
            .Section!
            .Blocks
            .FindIndex(b => b is { Layout.Name: "snippets_banner" });

        if (snippetsBannerIndex >= 0)
        {
            response.Section.Blocks.RemoveAt(snippetsBannerIndex);
            response.Section.Blocks.RemoveAt(snippetsBannerIndex); // excess separator
        }
    }

    private static void RemoveGarbageBlocks(ResponseData response) =>
        response.Section!.Blocks.RemoveAll(IsGarbageBlockPredicate);

    private static bool IsGarbageBlockPredicate(Block block)
    {
        return IsRadiostationsBlock(block) ||
               HasOnlyGarbageBanners(block) ||
               HasOnlyGarbageLinks(block);
    }

    private static bool IsRadiostationsBlock(Block block)
    {
        return block.DataType == "radiostations" ||
               block.Layout.Title == "Радиостанции" ||
               block.Layout.Title == "Эфиры";
    }

    private static bool HasOnlyGarbageBanners(Block block)
    {
        return block.Banners.Count > 0 &&
               block.Banners.RemoveAll(IsGarbageBanner) > 0 &&
               block.Banners.Count == 0;
    }

    private static bool IsGarbageBanner(CatalogBanner banner)
    {
        var url = banner.ClickAction?.Action.Url;

        if (url is null)
        {
            return false;
        }

        return url.Contains("subscription") ||
               url.Contains("combo") ||
               url.Contains("https://vk.com/app") ||
               url.Contains("https://vk.com/vk_music");
    }

    private static bool HasOnlyGarbageLinks(Block block)
    {
        return block.Links.Count > 0 &&
               block.Links.RemoveAll(IsGarbageLink) > 0 &&
               block.Links.Count == 0;
    }

    private static bool IsGarbageLink(Link link)
    {
        return link.Url.Contains("audio_offline") ||
               link.Url.Contains("radiostations") ||
               link.Url.Contains("music_transfer") ||
               link.Url.Contains("subscription");
    }

    private static void ProcessHorizontalButtons(ResponseData response)
    {
        var blocks = response.Section?.Blocks;
        if (blocks == null)
        {
            return;
        }

        for (var i = blocks.Count - 1; i >= 0; i--)
        {
            var block = blocks[i];
            if (IsHorizontalButtonsBlock(block))
            {
                var refBlockIndex = FindRefBlockIndex(blocks, block);
                if (refBlockIndex >= 0 && IsOpenSectionAction(block))
                {
                    MergeActions(blocks, refBlockIndex, block);
                    blocks.RemoveAt(i);
                }
            }
        }
    }

    private static bool IsHorizontalButtonsBlock(Block block)
    {
        return block.DataType == "action" && block.Layout?.Name == "horizontal_buttons";
    }

    private static int FindRefBlockIndex(List<Block> blocks, Block block)
    {
        return blocks.FindIndex(b =>
            b.DataType == block.Actions[0].RefDataType &&
            b.Layout?.Name == block.Actions[0].RefLayoutName) - 1;
    }

    private static bool IsOpenSectionAction(Block block)
    {
        return block.Actions[0].Action.Type == "open_section";
    }

    private static void MergeActions(List<Block> blocks, int refBlockIndex, Block block)
    {
        blocks[refBlockIndex].Actions.AddRange(block.Actions);
    }

    private static void ProcessSectionPlaylists(Block block, ResponseData response)
    {
        if (block.DataType == "music_recommended_playlists")
        {
            ProcessRecommendedPlaylists(block, response);
        }
        else
        {
            block.ProcessPlaylists(response.Playlists);
        }
    }

    //TODO: можно отрефакторить, но я уже устал
    // ReSharper disable once CognitiveComplexity
    private static void ProcessRecommendedPlaylists(Block block, ResponseData response)
    {
        foreach (var playlistId in block.PlaylistsIds)
        {
            var recommendedPlaylist = response.RecommendedPlaylists.Find(b => b.OwnerId + "_" + b.Id == playlistId);
            if (recommendedPlaylist == null) continue;

            var responsePlaylist = response.Playlists.Find(b => b.OwnerId + "_" + b.Id == playlistId);
            if (responsePlaylist == null) continue;

            recommendedPlaylist.Playlist = responsePlaylist;

            block.RecommendedPlaylists.Add(recommendedPlaylist);
        }

        foreach (var recommendedPlaylist in block.RecommendedPlaylists)
        {
            foreach (var audioId in recommendedPlaylist.AudiosIds)
            {
                var audio = response.Audios.Find(b => b.OwnerId + "_" + b.Id == audioId);
                if (audio == null) continue;

                recommendedPlaylist.Audios.Add(audio);
            }
        }
    }
}