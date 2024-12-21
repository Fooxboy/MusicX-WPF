using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Patches;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using Wpf.Ui.Controls;

namespace MusicX.Controls;

public partial class QueueTrackControl : UserControl
{
    private readonly PlayerService _player;

    public static readonly DependencyProperty IsInPlayerProperty = DependencyProperty.Register(
        nameof(IsInPlayer), typeof(bool), typeof(QueueTrackControl));

    public bool IsInPlayer
    {
        get => (bool)GetValue(IsInPlayerProperty);
        set => SetValue(IsInPlayerProperty, value);
    }

    public static readonly DependencyProperty IsCurrentlyPlayingProperty = DependencyProperty.Register(
        nameof(IsCurrentlyPlaying), typeof(bool), typeof(QueueTrackControl), new(PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not QueueTrackControl control) return;

        var sb = (Storyboard)control.FindResource(control.IsCurrentlyPlaying ? "PlayBorderFadeIn" : "PlayBorderFadeOut");
        sb.Begin();
    }

    public bool IsCurrentlyPlaying
    {
        get => (bool)GetValue(IsCurrentlyPlayingProperty);
        set => SetValue(IsCurrentlyPlayingProperty, value);
    }

    private int IndexInItemsControl =>
        ItemContainerGeneratorIndexHook.GetItemContainerIndex(((FrameworkElement)TemplatedParent).TemplatedParent ?? TemplatedParent);

    public QueueTrackControl()
    {
        InitializeComponent();
        _player = StaticService.Container.GetRequiredService<PlayerService>();
        _player.PlayStateChangedEvent += PlayerOnPlayStateChangedEvent;
    }

    private void PlayerOnPlayStateChangedEvent(object? sender, EventArgs e)
    {
        IsCurrentlyPlaying = _player.CurrentIndex == IndexInItemsControl;
        IconPlay.Symbol = _player.IsPlaying && IsCurrentlyPlaying ? SymbolRegular.Pause24 : SymbolRegular.Play24;
    }

    private void Track_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (IsCurrentlyPlaying)
        {
            if (_player.IsPlaying)
                _player.Pause();
            else
                _player.Play();
        }
        
        if (IsInPlayer)
        {
            _player.PlayTrackFromQueueAsync(IndexInItemsControl).SafeFireAndForget();
            return;
        }
        
        if (DataContext is not PlaylistTrack track) return;
        
        var vkService = StaticService.Container.GetRequiredService<VkService>();
        _player.PlayAsync(track.Data switch
        {
            VkTrackData vkData => vkData switch
            {
                { ParentBlockId: { } blockId } => new VkBlockPlaylist(vkService, blockId),
                { Playlist: { } vkPlaylist } => new VkPlaylistPlaylist(vkService, new(vkPlaylist.Id, vkPlaylist.OwnerId, vkPlaylist.AccessKey)),
                _ => new SinglePlaylist(track)
            },
            BoomTrackData => new SinglePlaylist(track),
            _ => throw new ArgumentOutOfRangeException()
        }).SafeFireAndForget();
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (IsCurrentlyPlaying) return;
        
        var sb = (Storyboard)FindResource("PlayBorderFadeIn");
        sb.Begin();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (IsCurrentlyPlaying) return;
        
        var sb = (Storyboard)FindResource("PlayBorderFadeOut");
        sb.Begin();
    }

    private void QueueTrackControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        PlayerOnPlayStateChangedEvent(_player, EventArgs.Empty);
    }

    private void QueueTrackControl_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _player.PlayStateChangedEvent -= PlayerOnPlayStateChangedEvent;
    }
}