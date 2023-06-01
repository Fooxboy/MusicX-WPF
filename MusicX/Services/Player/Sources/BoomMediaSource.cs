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

    public async Task<MediaPlaybackItem?> CreateMediaSourceAsync(PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is VkTrackData)
            return null;

        var ffSource = _currentSource = await FFmpegMediaSource.CreateFromUriAsync(track.Data.Url, new()
        {
            FFmpegOptions = new()
            {
                ["headers"] = $"Authorization: {_boomService.Client.DefaultRequestHeaders.Authorization}"
            }
        });

        return ffSource.CreateMediaPlaybackItem();
    }
}