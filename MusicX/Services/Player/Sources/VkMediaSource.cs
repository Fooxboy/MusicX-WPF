using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;
using NLog;
using WinRT;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    private readonly Logger _logger;

    public VkMediaSource(Logger logger)
    {
        _logger = logger;
    }

    public override async Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not VkTrackData { Url.Length: > 0 } vkData) return false;

        try
        {
            var rtMediaSource = await CreateWinRtMediaSource(vkData, cancellationToken: cancellationToken);
            
            RegisterSourceObjectReference(player, rtMediaSource);

            await rtMediaSource.OpenWithMediaPlayerAsync(player).AsTask(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use winrt decoder for vk media source");
            
            // i think its better to use task.run over task.yield because we aren't doing async with ffmpeg
            var playbackItem = await Task.Run(() =>
            {
                var file = MediaFile.Open(vkData.Url, MediaOptions);

                return CreateMediaPlaybackItem(file);
            }, cancellationToken);
            
            player.Source = playbackItem;
        }
        
        return true;
    }
}