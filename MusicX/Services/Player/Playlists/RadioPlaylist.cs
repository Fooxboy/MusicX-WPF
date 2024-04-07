using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

public record RadioData(Radio Playlist, BoomRadioType PlaylistType);

[JsonConverter(typeof(PlaylistJsonConverter<RadioPlaylist, RadioData>))]
public class RadioPlaylist : PlaylistBase<RadioData>
{
    private readonly BoomService _boomService;
    private bool _firstCall = true;
    
    public RadioPlaylist(BoomService boomService, RadioData radioData)
    {
        _boomService = boomService;
        Data = radioData;
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        if (!_firstCall) 
            return LoadAsyncInternal();
        
        _firstCall = false;
        return Data.Playlist.Tracks.Select(TrackExtensions.ToTrack).ToAsyncEnumerable();
    }

    private async IAsyncEnumerable<PlaylistTrack> LoadAsyncInternal()
    {
        var radio = Data.PlaylistType switch
        {
            BoomRadioType.Artist => await _boomService.GetArtistMixAsync(Data.Playlist.Artist.ApiId),
            BoomRadioType.Personal => await _boomService.GetPersonalMixAsync(),
            BoomRadioType.Tag => await _boomService.GetTagMixAsync(Data.Playlist.Tag.ApiId),
            _ => throw new ArgumentOutOfRangeException(null, "Unknown radio type")
        };

        foreach (var track in radio.Tracks)
        {
            yield return track.ToTrack();
        }
    }

    public override bool CanLoad => true;
    public override RadioData Data { get; }
}