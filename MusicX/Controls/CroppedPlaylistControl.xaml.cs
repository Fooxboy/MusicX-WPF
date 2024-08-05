using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player.Playlists;
using MusicX.Services.Player;
using MusicX.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using System;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для CroppedPlaylistControl.xaml
    /// </summary>
    public partial class CroppedPlaylistControl : UserControl
    {

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(CroppedPlaylistControl), new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty CoverProperty =
        DependencyProperty.Register("Cover", typeof(string), typeof(CroppedPlaylistControl), new PropertyMetadata(string.Empty));

        public string Cover
        {
            get { return (string)GetValue(CoverProperty); }
            set
            {
                SetValue(CoverProperty, value);
            }
        }

        public static readonly DependencyProperty PlaylistProperty =
          DependencyProperty.Register("Playlist", typeof(Playlist), typeof(CroppedPlaylistControl), new PropertyMetadata(new Playlist()));

        public Playlist Playlist
        {
            get { return (Playlist)GetValue(PlaylistProperty); }
            set
            {
                SetValue(PlaylistProperty, value);
            }
        }

        public CroppedPlaylistControl()
        {
            InitializeComponent();
        }

        private bool _nowPlay = false;

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(sender is Border border)
            {
                border.Opacity = 1;
            }
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 0.6;
            }
        }

        private void OpenFullPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var notificationService = StaticService.Container.GetRequiredService<NavigationService>();

            notificationService.OpenExternalPage(new PlaylistView(Playlist));
        }

        private async void PlayPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            try
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("PlayPlaylistWithButton", properties);

                var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
                connectionService.ReportMetric("PlayPlaylistWithButton");

                var playerService = StaticService.Container.GetRequiredService<PlayerService>();

                if (!_nowPlay)
                {
                    _nowPlay = true;

                    PlayIcon.Symbol = SymbolRegular.Timer24;
                    var vkService = StaticService.Container.GetRequiredService<VkService>();

                    await playerService.PlayAsync(
                        new VkPlaylistPlaylist(vkService, new(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey)));

                    PlayIcon.Symbol = SymbolRegular.Pause24;

                }
                else
                {
                    playerService.Pause();
                    PlayIcon.Symbol = SymbolRegular.Play24;

                    _nowPlay = false;
                }
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var player = StaticService.Container.GetRequiredService<PlayerService>();
            player.CurrentPlaylistChanged += PlayerOnCurrentPlaylistChanged;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var player = StaticService.Container.GetRequiredService<PlayerService>();
            player.CurrentPlaylistChanged -= PlayerOnCurrentPlaylistChanged;
        }

        private void PlayerOnCurrentPlaylistChanged(object? sender, EventArgs e)
        {
            if (sender is not PlayerService service)
                return;

            if (service.CurrentPlaylist is VkPlaylistPlaylist { Data: { } data } && data.PlaylistId == Playlist.Id)
            {
                _nowPlay = true;
                PlayIcon.Symbol = SymbolRegular.Pause24;
            }
            else
            {
                _nowPlay = false;
                PlayIcon.Symbol = SymbolRegular.Play24;
            }
        }

    }
}
