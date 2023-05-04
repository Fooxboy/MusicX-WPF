using MusicX.Avalonia.Audio.Services;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class QueueViewModel : ViewModelBase
{
    public QueueViewModel(IQueueService queueService, IPlayerService playerService)
    {
        QueueService = queueService;
        PlayerService = playerService;
    }

    public IQueueService QueueService { get; }
    public IPlayerService PlayerService { get; }
}