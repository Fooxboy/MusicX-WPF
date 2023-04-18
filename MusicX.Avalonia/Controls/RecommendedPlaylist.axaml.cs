using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MusicX.Avalonia.Controls;

public class RecommendedPlaylist : TemplatedControl
{
    public static readonly StyledProperty<MusicX.Avalonia.Core.Blocks.RecommendedPlaylist> PlaylistProperty =
        AvaloniaProperty.Register<RecommendedPlaylist, MusicX.Avalonia.Core.Blocks.RecommendedPlaylist>(
            "Playlist");

    public MusicX.Avalonia.Core.Blocks.RecommendedPlaylist Playlist
    {
        get => GetValue(PlaylistProperty);
        set => SetValue(PlaylistProperty, value);
    }
}