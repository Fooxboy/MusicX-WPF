using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;

namespace MusicX.Services.Player.Playlists;

public class RadioPlaylist : PlaylistBase<Radio>
{
    private readonly BoomService _boomService;
    private bool _firstCall = true;
    public RadioPlaylist(BoomService boomService, Radio data)
    {
        _boomService = boomService;
        Data = data;
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        if (!_firstCall) 
            return LoadAsyncInternal();
        
        _firstCall = false;
        return Data.Tracks.Select(TrackExtensions.ToTrack).ToAsyncEnumerable();
    }

    private async IAsyncEnumerable<PlaylistTrack> LoadAsyncInternal()
    {
        var radio = await _boomService.GetArtistMixAsync(Data.Artist.ApiId);
        
        foreach (var track in radio.Tracks)
        {
            yield return track.ToTrack();
        }
    }

    public override bool CanLoad => true;
    public override Radio Data { get; }
}