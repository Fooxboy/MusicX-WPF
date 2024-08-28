using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<SinglePlaylist, PlaylistTrack>))]
public class SinglePlaylist : PlaylistBase<PlaylistTrack>, IRandomAccessPlaylist, IShufflePlaylist
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

    public IPlaylist ShuffleWithSeed(int seed) => this;

    public ValueTask<int> GetCountAsync() => new(1); 

    public ValueTask<IEnumerable<PlaylistTrack>> GetRangeAsync(Range range)
    {
        return new([Data]);
    }
}