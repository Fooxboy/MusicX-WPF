using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<ListPlaylist, EquatableList<PlaylistTrack>>))]
public class ListPlaylist : PlaylistBase<EquatableList<PlaylistTrack>>, IRandomAccessPlaylist, IShufflePlaylist
{
    private bool _canLoad = true;

    public override bool CanLoad => _canLoad;

    public override EquatableList<PlaylistTrack> Data { get; }

    public ListPlaylist(IEnumerable<PlaylistTrack> data)
    {
        Data = new(data);
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        _canLoad = false;
        return Data.ToAsyncEnumerable();
    }

    public IPlaylist ShuffleWithSeed(int seed)
    {
        var data = Data.ToList();
        
        var random = new Random(seed);
        random.Shuffle(CollectionsMarshal.AsSpan(data));
        
        return new ListPlaylist(data);
    }

    public ValueTask<int> GetCountAsync() => new(Data.Count);

    public ValueTask<IEnumerable<PlaylistTrack>> GetRangeAsync(Range range)
    {
        return new(Data[range]);
    }
}

public class EquatableList<T> : List<T>, IEquatable<EquatableList<T>> where T : IEquatable<T>
{
    public EquatableList(IEnumerable<T> data) : base(data)
    {
    }
    
    public EquatableList()
    {
    }
    
    public bool Equals(EquatableList<T>? other)
    {
        return other is not null && Count == other.Count && other.SequenceEqual(this);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as EquatableList<T>);
    }

    public override int GetHashCode() => this.Select(b => b.GetHashCode()).Aggregate(HashCode.Combine);
}