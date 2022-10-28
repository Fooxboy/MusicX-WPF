using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Streaming.Adaptive;
using MusicX.Services.Player.Playlists;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : ITrackMediaSource
{
    public async Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (track.Data is VkTrackData vkData)
            {
                var source = await AdaptiveMediaSource.CreateFromUriAsync(new(vkData.Url));
                
                if (source.Status != AdaptiveMediaSourceCreationStatus.Success)
                    continue;

                var mediaSource = MediaSource.CreateFromAdaptiveMediaSource(source.MediaSource);
                return mediaSource;
            }

            break;
        }
        
        return null;
    }
}