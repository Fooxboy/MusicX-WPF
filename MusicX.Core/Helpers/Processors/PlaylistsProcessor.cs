using MusicX.Core.Models.General;

namespace MusicX.Core.Helpers.Processors;

internal static class PlaylistsProcessor
{
    internal static void Process(ResponseData response)
    {
        foreach (var playlist in response.Playlists)
        {
            var ownerId = playlist.Original?.OwnerId ?? playlist.OwnerId;
            playlist.OwnerName = GetOwnerName(ownerId, response);
        }
    }

    private static string GetOwnerName(long ownerId, ResponseData response)
    {
        if (ownerId < 0)
        {
            var id = ownerId * -1;
            var value = response.Groups.Find(g => g.Id == id);

            return value?.Name ?? throw new ArgumentException("Group not found");
        }
        else
        {
            var value = response.Profiles.Find(p => p.Id == ownerId);

            if (value is null)
            {
                throw new ArgumentException("Profile not found");
            }

            return value.FirstName + " " + value.LastName;
        }
    }
}