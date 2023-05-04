using System.Reactive.Concurrency;
using DynamicData;
using DynamicData.Binding;
using MusicX.Avalonia.Audio.Playlists;
using MusicX.Shared.Player;
using ReactiveUI;

namespace MusicX.Avalonia.Audio.Services;

public class QueueService : IQueueService
{
    private readonly IPlayerService _playerService;
    
    public IPlaylist? CurrentPlaylist { get; private set; }
    public IObservableCollection<PlaylistTrack> Queue { get; } = new ObservableCollectionExtended<PlaylistTrack>();

    public QueueService(IPlayerService playerService)
    {
        _playerService = playerService;
        _playerService.TrackEnded += PlayerServiceOnTrackEnded;
    }

    private void PlayerServiceOnTrackEnded(object? sender, EventArgs e)
    {
        Next();
    }

    public void Next()
    {
        var currentIndex = Queue.IndexOf(_playerService.CurrentTrack!);
        if (currentIndex + 1 >= Queue.Count)
            RxApp.TaskpoolScheduler.ScheduleAsync(LoadMoreAsync);
        else
            RxApp.TaskpoolScheduler.Schedule(() => _playerService.Play(Queue[currentIndex + 1]));
    }

    public void Previous()
    {
        var currentIndex = Queue.IndexOf(_playerService.CurrentTrack!);
        if (currentIndex > 0)
            _playerService.Play(Queue[currentIndex - 1]);
    }

    private async Task LoadMoreAsync(IScheduler arg1, CancellationToken token)
    {
        if (!CurrentPlaylist!.CanGetChunk)
        {
            CurrentPlaylist = null;
            Queue.Clear();
            _playerService.Stop();
            return;
        }

        Queue.AddRange(await CurrentPlaylist!.GetNextChunkAsync(token));
        Next();
    }

    public async ValueTask PlayPlaylistAsync(IPlaylist playlist, CancellationToken token, PlaylistTrack? track = null)
    {
        _playerService.Stop();
        
        Queue.Clear();
        Queue.AddRange(await playlist.GetNextChunkAsync(token));
        _playerService.Play(track ?? Queue[0]);

        CurrentPlaylist = playlist;
    }
}