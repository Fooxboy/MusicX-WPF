using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using WpfAnimatedGif;

namespace MusicX.Controls;

public partial class QueueTrackControl : UserControl
{
    private readonly PlayerService _player;

    public QueueTrackControl()
    {
        InitializeComponent();
        _player = StaticService.Container.GetRequiredService<PlayerService>();
        _player.PlayStateChangedEvent += PlayerOnPlayStateChangedEvent;
        _player.TrackChangedEvent += PlayerOnTrackChangedEvent;
    }

    private void PlayerOnTrackChangedEvent(object? sender, EventArgs e)
    {
        if (DataContext is PlaylistTrack track && _player.CurrentTrack == track)
        {
            IconPlay.Visibility = Visibility.Collapsed;

            UpdatePlayingAnimation(true);
            PanelAnim.Visibility = Visibility.Visible;
        }
        else
        {
            IconPlay.Visibility = Visibility.Visible;

            UpdatePlayingAnimation(false);
            PanelAnim.Visibility = Visibility.Collapsed;
        }
    }

    private void PlayerOnPlayStateChangedEvent(object? sender, EventArgs e)
    {
        if (DataContext is not PlaylistTrack track || _player.CurrentTrack != track) return;
        
        IconPlay.Symbol = _player.IsPlaying ? Wpf.Ui.Common.SymbolRegular.Pause24 : Wpf.Ui.Common.SymbolRegular.Play24;
        UpdatePlayingAnimation(_player.IsPlaying);
    }

    private void Track_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlaylistTrack track && _player.CurrentTrack != track)
            _player.PlayTrackFromQueueAsync(_player.Tracks.IndexOf(track)).SafeFireAndForget();
    }
    
    private void UpdatePlayingAnimation(bool autoStart)
    {
        if (ImageBehavior.GetAnimationController(PanelAnim) is { } controller)
        {
            if (autoStart)
            {
                controller.Play();
            }
            else
            {
                controller.Pause();
                controller.GotoFrame(0);
            }
            return;
        }
            
        ImageBehavior.SetAutoStart(PanelAnim, autoStart);
        ImageBehavior.SetAnimatedSource(PanelAnim, new BitmapImage(new("../Assets/play.gif", UriKind.Relative)));
    }
}