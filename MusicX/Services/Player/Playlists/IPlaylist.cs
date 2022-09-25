using System.Collections.Generic;

namespace MusicX.Services.Player.Playlists;

public interface IPlaylist<out TData> : IPlaylist where TData : class
{
    TData Data { get; }
}

public interface IPlaylist
{
    bool CanLoad { get; }
    IAsyncEnumerable<PlaylistTrack> LoadAsync();
}

public abstract class PlaylistBase<TData> : IPlaylist<TData> where TData : class
{
    public abstract IAsyncEnumerable<PlaylistTrack> LoadAsync();
    public abstract bool CanLoad { get; }
    public abstract TData Data { get; }
}