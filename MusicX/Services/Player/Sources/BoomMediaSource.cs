using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFmpegInteropX;
using MusicX.Core.Services;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : ITrackMediaSource
{
    private FFmpegMediaSource? _currentSource; // hold reference so it wont be collected before audio actually ends
    private readonly BoomService _boomService;

    public BoomMediaSource(BoomService boomService)
    {
        _boomService = boomService;
    }

    public async Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is VkTrackData)
            return null;

        var ffSource = _currentSource = await FFmpegMediaSource.CreateFromUriAsync(track.Data.Url, new()
        {
            DefaultBufferTimeUri = TimeSpan.FromMinutes(5),
            ReadAheadBufferEnabled = true,
            FFmpegOptions = new()
            {
                ["headers"] = $"Authorization: {_boomService.Client.DefaultRequestHeaders.Authorization}"
            }
        });
        
        ffSource.PlaybackSession = playbackSession;
        ffSource.StartBuffering();

        return ffSource.CreateMediaPlaybackItem();
    }
}