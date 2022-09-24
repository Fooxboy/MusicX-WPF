using MusicX.Services;
using NLog;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MusicX.Services.Player;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для FullScreenWindow.xaml
    /// </summary>
    public partial class FullScreenWindow : Window
    {
        private readonly Logger logger;
        private readonly PlayerService playerService;
        private readonly NotificationsService notificationsService;

        public FullScreenWindow(Logger logger, PlayerService playerService, NotificationsService notificationsService)
        {
            InitializeComponent();

            this.Loaded += FullScreenWindow_Loaded;
            this.logger = logger;
            this.playerService = playerService;
            this.notificationsService = notificationsService;

            this.playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
            playerService.PositionTrackChangedEvent += PlayerService_PositionTrackChangedEvent;
            playerService.NextTrackChanged += PlayerService_NextTrackChanged;
        }
        private void PlayerService_NextTrackChanged(object? sender, EventArgs e)
        {
            SetData();
        }

        private void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.SetData();
                PositionSlider.Maximum = playerService.CurrentTrack.Duration;

            });
        }

        private void FullScreenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetData();
        }

        private void SetData()
        {

            try
            {
                PositionSlider.Maximum = playerService.CurrentTrack.Duration;


                if (playerService.CurrentTrack.Album != null)
                {

                    var bitmapImage = new BitmapImage(new Uri(playerService.CurrentTrack.Album.Thumb.Photo600));
                    BackgroundImage.Source = bitmapImage;
                    CoverImage.ImageSource = bitmapImage;
                    CoverNote.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BackgroundImage.Source = null;
                    CoverImage.ImageSource = null;
                    CoverNote.Visibility = Visibility.Visible;
                }

                if (playerService.NextPlayTrack != null)
                {
                    if (playerService.NextPlayTrack.Album != null)
                    {
                        NextTrackCover.ImageSource = new BitmapImage(new Uri(playerService.NextPlayTrack.Album.Cover));
                        NextTrackNote.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        NextTrackCover.ImageSource = null;
                        NextTrackNote.Visibility = Visibility.Visible;
                    }

                    NextTrackName.Text = playerService.NextPlayTrack.Title;
                    string s2 = string.Empty;
                    if (playerService.NextPlayTrack.MainArtists != null)
                    {
                        foreach (var trackArtist in playerService.NextPlayTrack.MainArtists)
                        {
                            s2 += trackArtist.Name + ", ";
                        }

                        var artists2 = s2.Remove(s2.Length - 2);

                        NextTrackArtist.Text = artists2;
                    }
                    else
                    {
                        NextTrackArtist.Text = playerService.NextPlayTrack.Artist;
                    }
                }


                TrackName.Text = playerService.CurrentTrack.Title;
                string s = string.Empty;
                if (playerService.CurrentTrack.MainArtists != null)
                {
                    foreach (var trackArtist in playerService.CurrentTrack.MainArtists)
                    {
                        s += trackArtist.Name + ", ";
                    }

                    var artists = s.Remove(s.Length - 2);

                    ArtistName.Text = artists;
                }
                else
                {
                    ArtistName.Text = playerService.CurrentTrack.Artist;
                }
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Произошла ошибка", "MusicX не смог запустить полноэкранный режим");

                this.Close();

            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            await playerService.NextTrack();
        }

        private void PlayerService_PositionTrackChangedEvent(object? sender, TimeSpan e)
        {
            try
            {
                if (playerService == null) return;

                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    PositionSlider.Value = e.TotalSeconds;
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

            }

        }
       
        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playerService == null) return;

            playerService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
