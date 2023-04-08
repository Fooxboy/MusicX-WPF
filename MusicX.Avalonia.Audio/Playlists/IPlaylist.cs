using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Playlists;

public interface IPlaylist
{
    string? Title { get; }
    bool CanGetChunk { get; }
    ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token);
}