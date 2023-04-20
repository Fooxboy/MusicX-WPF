using DynamicData.Binding;
using MusicX.Avalonia.Audio.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Services;

public interface IQueueService
{
    IPlaylist? CurrentPlaylist { get; }
    IObservableCollection<PlaylistTrack> Queue { get; }
    void Next();
    void Previous();
    ValueTask PlayPlaylistAsync(IPlaylist playlist, CancellationToken token, PlaylistTrack? track = null);
}