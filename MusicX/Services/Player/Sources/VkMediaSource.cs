using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Streaming.Adaptive;
using MusicX.Services.Player.Playlists;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : ITrackMediaSource
{
    public async Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track)
    {
        return track.Data is VkTrackData { Url: { } url }
            ? MediaSource.CreateFromAdaptiveMediaSource((await AdaptiveMediaSource.CreateFromUriAsync(new(url))).MediaSource)
            : null;
    }
}