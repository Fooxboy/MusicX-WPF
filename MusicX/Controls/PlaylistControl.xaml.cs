using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Views;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        private readonly Logger logger;

        public PlaylistControl()
        {
            InitializeComponent();

            logger = StaticService.Container.GetRequiredService<Logger>();
        }

        public static readonly DependencyProperty PlaylistProperty =
          DependencyProperty.Register("Playlist", typeof(Playlist), typeof(PlaylistControl), new PropertyMetadata(new Playlist()));

        public Playlist Playlist
        {
            get { return (Playlist)GetValue(PlaylistProperty); }
            set
            {
                SetValue(PlaylistProperty, value);
            }
        }

        public static readonly DependencyProperty ShowFullProperty = DependencyProperty.Register(
            nameof(ShowFull), typeof(bool), typeof(PlaylistControl));

        public bool ShowFull
        {
            get => (bool)GetValue(ShowFullProperty);
            set => SetValue(ShowFullProperty, value);
        }

        public string ChartPosition { get; set; } = null;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                var player = StaticService.Container.GetRequiredService<PlayerService>();
                player.CurrentPlaylistChanged += PlayerOnCurrentPlaylistChanged;

                if (player.CurrentPlaylist is VkPlaylistPlaylist {Data: {} data} && data.PlaylistId == Playlist.Id)
                {
                    nowPlay = true;
                    iconPlay.Symbol = SymbolRegular.Pause24;
                }
                
                if (ShowFull)
                {
                    CompactGrid.Visibility = Visibility.Collapsed;
                    FullGrid.Visibility = Visibility.Visible;
                    Title.Text = Playlist.Title;
                    Year.Text = Playlist.Year.ToString();
                    var genres = string.Empty;
                    foreach (var genre in Playlist.Genres)
                    {
                        genres += $"{genre.Name}, ";
                    }

                    if(Playlist.Genres.Count > 0)
                    {
                        Genres.Text = genres.Remove(genres.Length - 2);

                    }

                    if(Playlist.Year == 0)
                    {
                        Year.Visibility = Visibility.Collapsed;
                        Genres.Visibility = Visibility.Collapsed;
                    }

                    if (Playlist.Cover != null)
                    {
                        CoverImage.ImageSource = new BitmapImage(new Uri(Playlist.Cover)) { DecodePixelHeight=85, DecodePixelWidth = 50, CacheOption = BitmapCacheOption.None };
                        //CoverImage.Source = Playlist.Cover;

                    }

                    if (Playlist.MainArtists != null && Playlist.MainArtists.Count > 0)
                    {
                        string s = string.Empty;
                        foreach (var trackArtist in Playlist.MainArtists)
                        {
                            s += trackArtist.Name + ", ";
                        }

                        var artists = s.Remove(s.Length - 2);

                        Artist.Text = artists;
                    }
                    else
                    {
                        Artist.Text = Playlist.OwnerName;
                    }

                    if (Playlist.Subtitle != null)
                    {
                        Artist.Text = Playlist.Subtitle;
                    }

                    return;
                }
                else
                {

                    if(ChartPosition != null)
                    {
                        Chart.Visibility = Visibility.Visible;
                        ChartPositionValue.Text = ChartPosition;
                    }

                    TitleCompact.Text = Playlist.Title;
                    if (Playlist.Cover != null)
                    {
                        CoverImageCompact.ImageSource = new BitmapImage(new Uri(Playlist.Cover)) { DecodePixelWidth= 2 , DecodePixelHeight = 2, CacheOption = BitmapCacheOption.None };

                    }

                    if (Playlist.MainArtists != null && Playlist.MainArtists.Count > 0)
                    {
                        string s = string.Empty;
                        foreach (var trackArtist in Playlist.MainArtists)
                        {
                            s += trackArtist.Name + ", ";
                        }

                        var artists = s.Remove(s.Length - 2);

                        ArtistCompact.Text = artists;
                    }
                    else
                    {
                       
                        ArtistCompact.Text = Playlist.OwnerName;

                    }

                    if(Playlist.Subtitle != null)
                    {
                        ArtistCompact.Text = Playlist.Subtitle;
                    }



                    //rectangle.Visibility = Visibility.Collapsed;
                   // PlaylistStackPanelCompact.Visibility = Visibility.Collapsed;

                    /*new Thread(() =>
                    {
                        var r = new Random();
                        var value = r.Next(800, 2000);

                        Thread.Sleep(value);


                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            rectangle.Visibility = Visibility.Visible;
                            PlaylistStackPanelCompact.Visibility = Visibility.Visible;
                            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
                            amim.Begin();
                        });


                    }).Start();*/
                }
            }catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Failed load playlist control");
                logger.Error(ex, ex.Message);
                this.Visibility = Visibility.Collapsed;
            }
        }
        private void PlayerOnCurrentPlaylistChanged(object? sender, EventArgs e)
        {
            if (sender is not PlayerService service || nowLoad)
                return;

            if (service.CurrentPlaylist is VkPlaylistPlaylist {Data: {} data} && data.PlaylistId == Playlist.Id)
            {
                nowPlay = true;
                iconPlay.Symbol = SymbolRegular.Pause24;
            }
            else
            {
                nowPlay = false;
                iconPlay.Symbol = SymbolRegular.Play24;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayPlaylistGrid.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayPlaylistGrid.Visibility = Visibility.Collapsed;
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
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

        private void FullGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var notificationService = StaticService.Container.GetRequiredService<NavigationService>();

            notificationService.OpenExternalPage(new PlaylistView(Playlist));
        }

        private void PlayPlaylistGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            //this.Cursor = Cursors.Hand;
            PlayButton.Opacity = 0.7;
        }

        private void PlayPlaylistGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            //this.Cursor = Cursors.Arrow;
            PlayButton.Opacity = 0.5;

        }

        bool nowPlay = false;
        private bool nowLoad;

        private async void PlayPlaylistGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

                var playerService = StaticService.Container.GetRequiredService<PlayerService>();

                if (!nowPlay)
                {
                    nowPlay = true;
                    nowLoad = true;

                    iconPlay.Symbol = SymbolRegular.Timer24;
                    var vkService = StaticService.Container.GetRequiredService<VkService>();

                    await playerService.PlayAsync(
                        new VkPlaylistPlaylist(vkService, new(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey)));

                    iconPlay.Symbol = SymbolRegular.Pause24;
                    nowLoad = false;
                }
                else
                {
                    playerService.Pause();
                    iconPlay.Symbol = SymbolRegular.Play24;

                    nowPlay = false;
                }
            }catch (Exception ex)
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

        private void cardAction_MouseEnter(object sender, MouseEventArgs e)
        {
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
            amim.Begin();
        }

        private void cardAction_MouseLeave(object sender, MouseEventArgs e)
        {
            var amim = (Storyboard)(this.Resources["CloseAnimation"]);
            amim.Begin();
        }

        private async void AddToLibrary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();

                await vkService.AddPlaylistAsync(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey);

                snackbarService.Show("Плейлист добавлен", "Плейлист теперь находится в Вашей библиотеке", ControlAppearance.Success);
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                snackbarService.ShowException("Мы не смогли добавить плейлист к Вам в библиотеку", ex);

            }

        }

        private async void AddToQueue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

            try
            {
                var playerService = StaticService.Container.GetRequiredService<PlayerService>();

                var vkService = StaticService.Container.GetRequiredService<VkService>();

                snackbarService.Show("Подождите", "Мы получаем треки из плейлиста и добавляем их в очередь", ControlAppearance.Caution);

                var result = await vkService.LoadFullPlaylistAsync( Playlist.Id, Playlist.OwnerId, Playlist.AccessKey);

                result.Audios.Reverse();
                var audios = result.Audios;
                foreach(var audio in audios)
                {
                    playerService.InsertToQueue(audio.ToTrack(result.Playlist), true);
                }

                snackbarService.Show("Готово!", "Треки из плейлиста добавлены в очередь!", ControlAppearance.Success);


            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                snackbarService.ShowException("Мы не смогли обновить очередь", ex);
            }
        }

        private void PlaylistControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            var player = StaticService.Container.GetRequiredService<PlayerService>();
            player.CurrentPlaylistChanged -= PlayerOnCurrentPlaylistChanged;
        }
    }
}
