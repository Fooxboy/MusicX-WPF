using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;
using NLog;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    private static readonly IReadOnlyDictionary<string, string> VkOptions;

    private readonly Logger _logger;

    static VkMediaSource()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        
        builder.Add("http_persistent", "false");
        
        VkOptions = builder.ToImmutable();
    }

    public VkMediaSource(Logger logger)
    {
        _logger = logger;
    }

    public override async Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not VkTrackData vkData) return null;

        if(string.IsNullOrEmpty(vkData.Url))
        {
            return null;
        }

        try
        {
            return await CreateWinRtMediaPlaybackItem(playbackSession, vkData, VkOptions);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use winrt decoder for vk media source");
            
            // i think its better to use task.run over task.yield because we aren't doing async with ffmpeg
            return await Task.Run(() =>
            {
                var file = MediaFile.Open(vkData.Url, MediaOptions);

                return CreateMediaPlaybackItem(file);
            }, cancellationToken);
        }
    }
}