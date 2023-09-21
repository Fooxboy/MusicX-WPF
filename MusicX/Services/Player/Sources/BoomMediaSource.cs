using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Core.Services;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : MediaSourceBase
{
    private readonly BoomService _boomService;

    private Stream? _currentStream;

    public BoomMediaSource(BoomService boomService)
    {
        _boomService = boomService;
    }

    public override async Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default)
    {
        if (track.Data is not BoomTrackData boomData)
            return null;

        if (CurrentSource != null)
            lock (CurrentSource)
            {
                CurrentSource.Dispose();
                CurrentSource = null;
            }
        
        if (_currentStream != null)
            await _currentStream.DisposeAsync();

        var stream = _currentStream = await _boomService.Client.GetStreamAsync(boomData.Url, cancellationToken);

        var file = CurrentSource = MediaFile.Open(stream, MediaOptions);
            
        return CreateMediaPlaybackItem(file);
    }
}