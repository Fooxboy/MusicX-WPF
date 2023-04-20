using System.Reactive.Linq;
using Windows.Media.Playback;
using DynamicData.Binding;
using MusicX.Avalonia.Audio.Services;
using ReactiveUI;

namespace MusicX.Avalonia.Audio.Windows;

public class WindowsQueueService : QueueService
{
    public WindowsQueueService(IPlayerService playerService) : base(playerService)
    {
        if (playerService is WindowsPlayerService windowsPlayerService) AttachToPlayer(windowsPlayerService);
    }

    private void AttachToPlayer(WindowsPlayerService playerService)
    {
        var commandManager = playerService.Player.CommandManager;

        playerService.WhenValueChanged(x => x.CurrentTrack)
                     .Select(b => b is not null && Queue.IndexOf(b) + 1 < Queue.Count)
                     .Select(b => b ? MediaCommandEnablingRule.Always : MediaCommandEnablingRule.Never)
                     .BindTo(commandManager.NextBehavior, x => x.EnablingRule);
        
        playerService.WhenValueChanged(x => x.CurrentTrack)
                     .Select(b => b is not null && Queue.IndexOf(b) > 0)
                     .Select(b => b ? MediaCommandEnablingRule.Always : MediaCommandEnablingRule.Never)
                     .BindTo(commandManager.PreviousBehavior, x => x.EnablingRule);
        
        commandManager.NextReceived += CommandManagerOnNextReceived;
        commandManager.PreviousReceived += CommandManagerOnPreviousReceived;
    }

    private void CommandManagerOnPreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
    {
        Previous();
    }

    private void CommandManagerOnNextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
    {
        Next();
    }
}