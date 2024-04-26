using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using FFmpegInteropX;
using MusicX.Core.Services;
using MusicX.Shared.Player;
using NLog;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : MediaSourceBase
{
    private readonly BoomService _boomService;
    private readonly Logger _logger;

    public BoomMediaSource(BoomService boomService, Logger logger)
    {
        _boomService = boomService;
        _logger = logger;
    }

    public override async Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not BoomTrackData boomData)
            return false;
        
        try
        {
            var rtMediaSource = await CreateWinRtMediaSource(boomData,
                new Dictionary<string, string>
                {
                    ["headers"] = $"Authorization: {_boomService.Client.DefaultRequestHeaders.Authorization}"
                }, cancellationToken);
            
            await rtMediaSource.OpenWithMediaPlayerAsync(player).AsTask(cancellationToken);
            
            RegisterSourceObjectReference(player, rtMediaSource);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use winrt decoder for boom media source");
            
            var response = await _boomService.Client.GetAsync(boomData.Url, cancellationToken);

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            player.Source = new MediaPlaybackItem(MediaSource.CreateFromStream(stream.AsRandomAccessStream(),
                response.Content.Headers.ContentType?.MediaType ??
                "audio/mpeg"));
        }
        
        return true;
    }
}