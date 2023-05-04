using System.Reactive;
using System.Reactive.Linq;
using MusicX.Avalonia.Audio.Services;
using ReactiveUI;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class PlaybackViewModel : ViewModelBase
{
    public IPlayerService PlayerService { get; }
    
    private readonly IQueueService _queueService;
    private readonly IServiceProvider _serviceProvider;

    public PlaybackViewModel(IPlayerService playerService, IQueueService queueService, IServiceProvider serviceProvider)
    {
        PlayerService = playerService;
        _queueService = queueService;
        _serviceProvider = serviceProvider;

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
        OpenQueueCommand = ReactiveCommand.Create(OpenQueue);
    }

    private void OpenQueue()
    {
        var viewModel = (QueueViewModel) _serviceProvider.GetService(typeof(QueueViewModel))!;
        
        MessageBus.Current.SendMessage<ViewModelBase>(viewModel, "nav");
    }

    public ReactiveCommand<Unit,Unit> OpenQueueCommand { get; set; }
    public ReactiveCommand<Unit,Unit> PrevTrackCommand { get; set; }

    public ReactiveCommand<Unit,Unit> NextTrackCommand { get; set; }

    public ReactiveCommand<Unit,Unit> PlayPauseCommand { get; set; }

    public TimeSpan Elapsed { get; set; }
}