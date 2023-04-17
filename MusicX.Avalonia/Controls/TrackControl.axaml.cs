using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using DynamicData.Binding;
using MusicX.Avalonia.Audio.Services;
using MusicX.Shared.Player;
using ReactiveUI;

namespace MusicX.Avalonia.Controls;

[PseudoClasses(":playing")]
public class TrackControl : TemplatedControl
{
    public static readonly StyledProperty<PlaylistTrack> TrackProperty =
        AvaloniaProperty.Register<TrackControl, PlaylistTrack>(
            "Track");

    public PlaylistTrack Track
    {
        get => GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
    }

    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<TrackControl, ICommand>(
        "Command");

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<TrackControl, object?>(
            "CommandParameter");

    private IDisposable? _disposable;

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _disposable = App.Provider.GetService<PlayerService>()
                         .WhenValueChanged(x => x.CurrentTrack)
                         .ObserveOn(RxApp.MainThreadScheduler)
                         .Subscribe(track =>
                         {
                             if (track == Track)
                                 PseudoClasses.Add(":playing");
                             else
                                 PseudoClasses.Remove(":playing");
                         });
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();
        _disposable?.Dispose();
    }
}