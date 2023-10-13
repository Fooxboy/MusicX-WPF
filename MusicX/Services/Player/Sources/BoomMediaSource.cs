using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using FFMediaToolkit.Decoding;
using MusicX.Core.Services;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : MediaSourceBase
{
    private readonly BoomService _boomService;

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

        /*var stream = await _boomService.Client.GetStreamAsync(boomData.Url, cancellationToken);

        var file = MediaFile.Open(stream, MediaOptions);
            
        return CreateMediaPlaybackItem(file);*/
        
        var response = await _boomService.Client.GetAsync(boomData.Url, cancellationToken);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return new(MediaSource.CreateFromStream(stream.AsRandomAccessStream(),
            response.Content.Headers.ContentType?.MediaType ??
            "audio/mpeg"));
    }
}