using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;
using System.IO;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    public override Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not VkTrackData vkData) return Task.FromResult<MediaPlaybackItem?>(null);

        // i think its better to use task.run over task.yield because we aren't doing async with ffmpeg
        return Task.Run(() =>
        {
            if(string.IsNullOrEmpty(vkData.Url))
            {
                return null;
            }

            var file = MediaFile.Open(vkData.Url, MediaOptions);

            return CreateMediaPlaybackItem(file);
        }, cancellationToken)!;
    }
}