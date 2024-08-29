using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Wpf.Ui.Controls;
using System.Globalization;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для RecommendedPlaylistControl.xaml
    /// </summary>
    public partial class RecommendedPlaylistControl : UserControl
    {
        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            nameof(Playlist), typeof(RecommendedPlaylist), typeof(RecommendedPlaylistControl));

        public RecommendedPlaylist Playlist
        {
            get => (RecommendedPlaylist)GetValue(PlaylistProperty);
            set => SetValue(PlaylistProperty, value);
        }
        public RecommendedPlaylistControl()
        {
            InitializeComponent();
        }

        private void RecommendedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                double value;

                try
                {
                    value = Convert.ToDouble(Playlist.Percentage, CultureInfo.InvariantCulture);

                }catch(Exception ex)
                {
                    value = Convert.ToDouble(Playlist.Percentage.Replace('.', ','));
                }

                value = Math.Round(value, 2);
                Percent.Text = $"{Math.Truncate(value * 100)}%";
                Subtitle.Text = Playlist.PercentageTitle;

                NamePlaylist.Text = Playlist.Playlist.Title;
                OwnerName.Text = Playlist.Playlist.OwnerName;

                if(Playlist.Cover != null)
                {
                    var image = new ImageBrush();
                    image.ImageSource = new BitmapImage(new Uri(Playlist.Cover));
                    BackgroundRectangle.Fill = image;
                    OwnerName.Visibility = Visibility.Collapsed;
                }else
                {
                    var brr = (SolidColorBrush)new BrushConverter().ConvertFrom(Playlist.Color);

                    var brrTwo = brr.Clone();

                    Color color = brrTwo.Color;
                    color.R += 50;
                    color.B += 20;
                    color.G += 72;

                    GradientBackground.GradientStops.Add(new GradientStop() { Color = brr.Color, Offset = 0.0 });
                    GradientBackground.GradientStops.Add(new GradientStop() { Color = color, Offset = 1.1 });
                }

                //BackgroundRectangle.Fill =
                
                var player = StaticService.Container.GetRequiredService<PlayerService>();
                player.CurrentPlaylistChanged += PlayerOnCurrentPlaylistChanged;

                if (player.CurrentPlaylist is VkPlaylistPlaylist {Data: {} data} && data.PlaylistId == Playlist.Id)
                {
                    nowPlay = true;
                    Icons.Symbol = SymbolRegular.Pause24;
                }
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, "Failed to load recommended playlist");
            }
            

        }
        private void PlayerOnCurrentPlaylistChanged(object? sender, EventArgs e)
        {
            if (sender is not PlayerService service)
                return;

            if (service.CurrentPlaylist is VkPlaylistPlaylist {Data: {} data} && data.PlaylistId == Playlist.Id)
            {
                nowPlay = true;
                Icons.Symbol = SymbolRegular.Pause24;
            }
            else
            {
                Icons.Symbol = SymbolRegular.Play24;
            }
        }

        private void TitleCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
            connectionService.ReportMetric("OpenPlayList", "RecommendedPlaylist");
            
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

            navigationService.OpenExternalPage(new PlaylistView(Playlist.Playlist.Id,Playlist.Playlist.OwnerId , Playlist.Playlist.AccessKey));
        }

        private void PlayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Opacity = 0.5;

        }

        private void PlayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Opacity = 0.2;
        }


        bool nowLoad = false;
        bool nowPlay = false;
        private async void PlayButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                nowLoad = true;

                var playerService = StaticService.Container.GetRequiredService<PlayerService>();
                var vkService = StaticService.Container.GetRequiredService<VkService>();

                if (!nowPlay)
                {
                    nowPlay = true;

                    Icons.Symbol = SymbolRegular.Timer20;
                    
                    await playerService.PlayAsync(new VkPlaylistPlaylist(
                                                      vkService,
                                                      new(Playlist.Playlist.Id, Playlist.Playlist.OwnerId,
                                                          Playlist.Playlist.AccessKey)));

                    Icons.Symbol = SymbolRegular.Pause24;

                    nowLoad = false;

                }
                else
                {
                    playerService.Pause();
                    Icons.Symbol = SymbolRegular.Play24;

                    await Task.Delay(400);
                    nowLoad = false;

                    nowPlay = false;

                }
            }
            catch (Exception ex)
            {
                nowLoad = false;
                
                var logger = StaticService.Container.GetRequiredService<Logger>();
                
                logger.Error(ex, "Failed to play recommended playlist");
            }
        }

        private void RecommendedPlaylistControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            var player = StaticService.Container.GetRequiredService<PlayerService>();
            player.CurrentPlaylistChanged -= PlayerOnCurrentPlaylistChanged;
        }
    }
}
