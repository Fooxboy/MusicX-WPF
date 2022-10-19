using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media.Core;
using MusicX.Core.Services;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public class BoomMediaSource : ITrackMediaSource
{
    private readonly BoomService _boomService;

    public BoomMediaSource(BoomService boomService)
    {
        _boomService = boomService;
    }

    public async Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track)
    {
        if (track.Data is VkTrackData)
            return null;

        var response = await _boomService.Client.GetAsync(track.Data.Url);

        var stream = await response.Content.ReadAsStreamAsync();

        return MediaSource.CreateFromStream(stream.AsRandomAccessStream(),
                                            response.Content.Headers.ContentType?.MediaType ??
                                            "audio/mpeg");
    }
}