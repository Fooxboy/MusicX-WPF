using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<SinglePlaylist, PlaylistTrack>))]
public class SinglePlaylist : PlaylistBase<PlaylistTrack>
{
    private bool _canLoad = true;

    public override bool CanLoad => _canLoad;

    public override PlaylistTrack Data { get; }

    public SinglePlaylist(PlaylistTrack data)
    {
        Data = data;
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        _canLoad = false;
        return new List<PlaylistTrack> { Data }.ToAsyncEnumerable();
    }
}