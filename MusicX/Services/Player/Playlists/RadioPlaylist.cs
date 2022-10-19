using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

public class RadioPlaylist : PlaylistBase<Radio>
{
    private readonly BoomService _boomService;
    private bool _firstCall = true;
    private BoomRadioType _radioType;

    public RadioPlaylist(BoomService boomService, Radio data, BoomRadioType radioType)
    {
        _boomService = boomService;
        Data = data;
        _radioType = radioType;
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
        Radio radio;
        if(_radioType == BoomRadioType.Artist)
        {
            radio = await _boomService.GetArtistMixAsync(Data.Artist.ApiId);
        }else if(_radioType == BoomRadioType.Personal)
        {
            radio = await _boomService.GetPersonalMixAsync();
        }else if(_radioType == BoomRadioType.Tag)
        {
            radio = await _boomService.GetTagMixAsync(Data.Tag.ApiId);
        }else
        {
            radio = null;
        }

        foreach (var track in radio.Tracks)
        {
            yield return track.ToTrack();
        }
    }

    public override bool CanLoad => true;
    public override Radio Data { get; }
}