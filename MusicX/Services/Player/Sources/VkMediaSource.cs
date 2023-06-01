using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFmpegInteropX;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : ITrackMediaSource
{
    private FFmpegMediaSource? _currentSource; // hold reference so it wont be collected before audio actually ends
    
    public async Task<MediaPlaybackItem?> CreateMediaSourceAsync(PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (track.Data is not VkTrackData vkData) break;

            var ffSource = _currentSource = await FFmpegMediaSource.CreateFromUriAsync(vkData.Url, new()
            {
                FFmpegOptions = new()
                {
                    ["http_persistent"] = "false"
                }
            });

            return ffSource.CreateMediaPlaybackItem();
        }
        
        return null;
    }
}