using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Playlists;

public class SinglePlaylist : IPlaylist
{
    private readonly PlaylistTrack _track;
    public string? Title => null;

    public bool CanGetChunk => false;
    
    public SinglePlaylist(PlaylistTrack track)
    {
        _track = track;
    }

    public ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token)
    {
        return new(new[] { _track });
    }
}