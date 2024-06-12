using MusicX.Core.Models.General;

namespace MusicX.Core.Helpers.Processors;

internal static class ReplacementsProcessor
{
    internal static void Process(ResponseData response)
    {
        if (response.Replacements == null)
        {
            return;
        }

        var blockReplacements = response
            .Replacements
            .ReplacementsModels
            .SelectMany(replaceModel => replaceModel.ToBlocks);

        foreach (var block in blockReplacements)
        {
            block.ProcessAudios(response.Audios);
            block.ProcessPlaylists(response.Playlists);
            block.ProcessCatalogBanners(response.CatalogBanners);
            block.ProcessLinks(response.Links);
            block.ProcessSuggestions(response.Suggestions);
            block.ProcessArtists(response.Artists);
            block.ProcessTexts(response.Texts);
            block.ProcessGroups(response.Groups);
            block.ProcessCurators(response.Curators);
            block.ProcessMusicOwners(response.MusicOwners);
            block.ProcessFollowingsUpdateInfos(response.FollowingsUpdateInfos);
        }
    }
}