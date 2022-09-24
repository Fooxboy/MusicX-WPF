using System.Windows.Controls;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;

namespace MusicX.Controls;

public partial class QueueTrackControl : UserControl
{
    private readonly PlayerService _player;

    public QueueTrackControl()
    {
        InitializeComponent();
        _player = StaticService.Container.GetRequiredService<PlayerService>();
    }

    private void Track_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlaylistTrack track && _player.CurrentTrack != track)
            _player.PlayTrackFromQueueAsync(_player.Tracks.IndexOf(track)).SafeFireAndForget();
    }
}