using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MusicX.Core.Models;

namespace MusicX.Controls.Blocks;

public partial class RecommsPlaylistBlock : UserControl
{
    public static readonly DependencyProperty PlaylistsProperty = DependencyProperty.Register(
        nameof(Playlists), typeof(IEnumerable<Playlist>), typeof(RecommsPlaylistBlock));

    public IEnumerable<Playlist> Playlists
    {
        get => (IEnumerable<Playlist>)GetValue(PlaylistsProperty);
        set => SetValue(PlaylistsProperty, value);
    }
    
    public RecommsPlaylistBlock()
    {
        InitializeComponent();
    }
}