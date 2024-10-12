using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Models.Enums;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks;

public sealed partial class AudioMixesBlock : UserControl
{

    public MixMode Mode
    {
        get => (MixMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register("Mode", typeof(MixMode), typeof(AudioMixesBlock));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register("IsPlaying", typeof(bool), typeof(AudioMixesBlock));
    private readonly PlayerService _player;

    public AudioMixesBlock()
    {
        InitializeComponent();

        Mode = MixMode.Mix;

        _player = StaticService.Container.GetRequiredService<PlayerService>();

        _player.CurrentPlaylistChanged += Player_CurrentPlaylistChanged;
        _player.PlayStateChangedEvent += Player_CurrentPlaylistChanged;
        Player_CurrentPlaylistChanged(_player, EventArgs.Empty);
    }

    private void Player_CurrentPlaylistChanged(object? sender, EventArgs e)
    {
        IsPlaying = _player.CurrentPlaylist is MixPlaylist && _player.IsPlaying;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (IsPlaying)
        {
            _player.Pause();
            return;
        }

        if (_player.CurrentPlaylist is MixPlaylist)
        {
            _player.Play();
            return;
        }

        var data = new MixOptions("common");

        await _player.PlayAsync(new MixPlaylist(data, StaticService.Container.GetRequiredService<VkService>()));
    }

    private void LibraryButton_Click(object sender, RoutedEventArgs e)
    {
        Mode = MixMode.Library;

        MixButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent;

        LibraryButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Secondary;
    }

    private void MixButton_Click(object sender, RoutedEventArgs e)
    {
        Mode = MixMode.Mix;

        MixButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Secondary;

        LibraryButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
    }
}
