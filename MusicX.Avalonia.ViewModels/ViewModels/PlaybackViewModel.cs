using System.Reactive;
using System.Reactive.Linq;
using MusicX.Avalonia.Audio.Services;
using ReactiveUI;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class PlaybackViewModel : ViewModelBase
{
    public PlayerService PlayerService { get; }
    
    private readonly QueueService _queueService;

    public PlaybackViewModel(PlayerService playerService, QueueService queueService)
    {
        PlayerService = playerService;
        _queueService = queueService;

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                  .ObserveOn(RxApp.TaskpoolScheduler)
                  .Select(_ => PlayerService.CurrentTrack is null ? TimeSpan.Zero : PlayerService.Elapsed)
                  .BindTo(this, x => x.Elapsed);

        PlayPauseCommand = ReactiveCommand.Create(() =>
        {
            if (PlayerService.CurrentTrack is null)
                return;
            if (PlayerService.IsPlaying)
                PlayerService.Pause();
            else PlayerService.Play(PlayerService.CurrentTrack);
        });
        NextTrackCommand = ReactiveCommand.Create(_queueService.Next);
        PrevTrackCommand = ReactiveCommand.Create(_queueService.Previous);
    }

    public ReactiveCommand<Unit,Unit> PrevTrackCommand { get; set; }

    public ReactiveCommand<Unit,Unit> NextTrackCommand { get; set; }

    public ReactiveCommand<Unit,Unit> PlayPauseCommand { get; set; }

    public TimeSpan Elapsed { get; set; }
}