using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFmpegInteropX;
using MusicX.Shared.Player;
using NLog;
using LogLevel = FFmpegInteropX.LogLevel;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : ITrackMediaSource
{
    private FFmpegMediaSource? _currentSource; // hold reference so it wont be collected before audio actually ends

    public VkMediaSource(Logger logger)
    {
#if DEBUG
        FFmpegInteropLogging.SetLogLevel(LogLevel.Info);
#else
        FFmpegInteropLogging.SetLogLevel(LogLevel.Warning);
#endif
        FFmpegInteropLogging.SetLogProvider(new NLogLogProvider(logger));
    }
    
    public async Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (track.Data is not VkTrackData vkData) break;

            var ffSource = _currentSource = await FFmpegMediaSource.CreateFromUriAsync(vkData.Url, new()
            {
                DefaultBufferTimeUri = TimeSpan.FromMinutes(5),
                ReadAheadBufferEnabled = true,
                FFmpegOptions = new()
                {
                    ["http_persistent"] = "false"
                }
            });

            ffSource.PlaybackSession = playbackSession;
            ffSource.StartBuffering();
            
            return ffSource.CreateMediaPlaybackItem();
        }
        
        return null;
    }
}

internal class NLogLogProvider : ILogProvider
{
    private readonly Logger _logger;

    public NLogLogProvider(Logger logger)
    {
        _logger = logger;
    }

    public void Log(LogLevel level, string message)
    {
        _logger.Log(level switch
        {
            LogLevel.Trace => NLog.LogLevel.Trace,
            LogLevel.Verbose => NLog.LogLevel.Debug,
            LogLevel.Debug => NLog.LogLevel.Debug,
            LogLevel.Info => NLog.LogLevel.Info,
            LogLevel.Warning => NLog.LogLevel.Warn,
            LogLevel.Error => NLog.LogLevel.Error,
            LogLevel.Fatal => NLog.LogLevel.Fatal,
            LogLevel.Panic => NLog.LogLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        }, message);
    }
}