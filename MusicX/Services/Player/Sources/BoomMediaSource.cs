using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using MusicX.Core.Services;
using MusicX.Services.Player.Playlists;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : ITrackMediaSource
{
    private readonly BoomService _boomService;

    public BoomMediaSource(BoomService boomService)
    {
        _boomService = boomService;
    }

    public async Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track, CancellationToken cancellationToken = default)
    {
        if (track.Data is VkTrackData)
            return null;

        var response = await _boomService.Client.GetAsync(track.Data.Url, cancellationToken);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return MediaSource.CreateFromStream(stream.AsRandomAccessStream(),
                                            response.Content.Headers.ContentType?.MediaType ??
                                            "audio/mpeg");
    }
}