using MusicX.Core.Models.General;

namespace MusicX.Core.Helpers.Processors;

internal static class BlocksProcessor
{
    public static void Process(ResponseData response)
    {
        if (response.Block == null)
        {
            return;
        }

        var block = response.Block;

        block.ProcessAudios(response.Audios);
        block.ProcessPlaylists(response.Playlists);
        block.ProcessCatalogBanners(response.CatalogBanners);
        block.ProcessLinks(response.Links);
        block.ProcessSuggestions(response.Suggestions);
        block.ProcessPlaceholders(response.Placeholders);
        block.ProcessArtists(response.Artists);
        block.ProcessTexts(response.Texts);
        block.ProcessGroups(response.Groups);
        block.ProcessCurators(response.Curators);
        block.ProcessMusicOwners(response.MusicOwners);
        block.ProcessFollowingsUpdateInfos(response.FollowingsUpdateInfos);
    }
}