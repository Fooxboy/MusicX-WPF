using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels;
using MusicX.Views;
using Wpf.Ui.Controls;

namespace MusicX.Controls;

public partial class RecommsPlaylist : UserControl
{
    public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(nameof(Playlist),
        typeof(Playlist), typeof(RecommsPlaylist));

    private PlayerService _playerService = null!;
    private SymbolIcon _playPauseIcon = null!;

    public Playlist Playlist
    {
        get => (Playlist)GetValue(PlaylistProperty);
        set => SetValue(PlaylistProperty, value);
    }
    
    public RecommsPlaylist()
    {
        InitializeComponent();
    }

    private bool _wasClicked;

    private void PlayerServiceOnCurrentPlaylistChanged(object? sender, EventArgs e)
    {
        if (_wasClicked)
            return;
        
        if (_playerService.CurrentPlaylist is VkPlaylistPlaylist vkPlaylistPlaylist && vkPlaylistPlaylist.Data ==
            new PlaylistData(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey))
            _playPauseIcon.Symbol = SymbolRegular.Pause48;
        else _playPauseIcon.Symbol = SymbolRegular.Play48;
    }

    private async void PlayPause_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _playPauseIcon.Symbol = SymbolRegular.Timer48;
        _wasClicked = true;

        try
        {
            await _playerService.PlayAsync(new VkPlaylistPlaylist(StaticService.Container.GetRequiredService<VkService>(),
                new(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey)));
        }
        finally
        {
            _wasClicked = false;
            PlayerServiceOnCurrentPlaylistChanged(_playerService, EventArgs.Empty);
        }
    }

    private void PlayPauseOverlay_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.Source != sender)
            return;
        
        var properties = new Dictionary<string, string>
        {
#if DEBUG
            { "IsDebug", "True" },
#endif
            {"Version", StaticService.Version }
        };
        Analytics.TrackEvent("OpenPlaylist", properties);
            
        var notificationService = StaticService.Container.GetRequiredService<NavigationService>();

        notificationService.OpenExternalPage(new PlaylistView(Playlist));
    }

    private void RecommsPlaylist_OnLoaded(object sender, RoutedEventArgs e)
    {
        _playPauseIcon = (SymbolIcon)Template.FindName("PlayPauseIcon", this)!;
        _playerService = StaticService.Container.GetRequiredService<PlayerService>();
        _playerService.CurrentPlaylistChanged += PlayerServiceOnCurrentPlaylistChanged;
        PlayerServiceOnCurrentPlaylistChanged(_playerService, EventArgs.Empty);
    }
}