using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Controls;

public class TrackControl : TemplatedControl
{
    public static readonly StyledProperty<PlaylistTrack> TrackProperty = AvaloniaProperty.Register<TrackControl, PlaylistTrack>(
        "Track");

    public PlaylistTrack Track
    {
        get => GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
    }
}