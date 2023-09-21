using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    public override Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not VkTrackData vkData) return Task.FromResult<MediaPlaybackItem?>(null);

        if (CurrentSource != null)
            lock (CurrentSource)
            {
                CurrentSource.Dispose();
                CurrentSource = null;
            }

        var file = CurrentSource = MediaFile.Open(vkData.Url, MediaOptions);
            
        return Task.FromResult(CreateMediaPlaybackItem(file))!;
    }
}