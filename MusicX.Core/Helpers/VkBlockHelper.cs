using MusicX.Core.Models.General;
using MusicX.Core.Helpers.Processors;

namespace MusicX.Core.Helpers;

public static class VkBlockHelper
{
    public static ResponseData Process(this ResponseData? response)
    {
        if (response?.Playlists == null)
        {
            return null!;
        }

        PlaylistsProcessor.Process(response);
        ReplacementsProcessor.Process(response);
        BlocksProcessor.Process(response);
        SectionProcessor.Process(response);

        return response;
    }
}