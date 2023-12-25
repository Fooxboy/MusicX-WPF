using System;
using System.Collections.Generic;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

public interface IPlaylist<out TData> : IPlaylist where TData : class, IEquatable<TData>
{
    TData Data { get; }
}

public interface IPlaylist : IEquatable<IPlaylist>
{
    bool CanLoad { get; }
    IAsyncEnumerable<PlaylistTrack> LoadAsync();
}

public abstract class PlaylistBase<TData> : IPlaylist<TData> where TData : class, IEquatable<TData>
{
    public abstract IAsyncEnumerable<PlaylistTrack> LoadAsync();
    public abstract bool CanLoad { get; }
    public abstract TData Data { get; }
    
    public bool Equals(IPlaylist? other)
    {
        return other is PlaylistBase<TData> { Data: { } otherData } && Data.Equals(otherData);
    }

    public override bool Equals(object? obj) => Equals((IPlaylist?)obj);
    
    public override int GetHashCode() => Data.GetHashCode();
}