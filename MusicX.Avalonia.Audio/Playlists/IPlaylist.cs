using System.ComponentModel;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Playlists;

public interface IPlaylist : INotifyPropertyChanged
{
    string? Title { get; }
    bool CanGetChunk { get; }
    ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token);
}