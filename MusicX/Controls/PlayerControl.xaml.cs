using DryIoc;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {

        private readonly PlayerService playerService;
        private readonly Logger logger;
        private ConfigModel config;
        public PlayerControl()
        {
            InitializeComponent();

            this.playerService = StaticService.Container.Resolve<PlayerService>();
            this.logger = StaticService.Container.Resolve<Logger>();
            playerService.PlayStateChangedEvent += PlayerService_PlayStateChangedEvent;
            playerService.PositionTrackChangedEvent += PlayerService_PositionTrackChangedEvent;
            playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;

            this.MouseWheel += PlayerControl_MouseWheel;
            
        }

        private void PlayerControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!mouseEnteredInVolume) return;

            if (playerService == null) return;

            var delta = e.Delta/1000d;
            Volume.Value += delta;
        }

        private async void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            try
            {

                if (playerService == null) return;

                if (playerService.CurrentTrack.IsExplicit)
                {
                    explicitBadge.Visibility = Visibility.Visible;

                }
                else
                {
                    explicitBadge.Visibility = Visibility.Collapsed;

                }

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    TrackTitle.Text = playerService.CurrentTrack.Title;
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


                    TimeSpan t = playerService.Position;
                    if (t.Hours > 0)
                        CurrentPosition.Text = t.ToString("h\\:mm\\:ss");
                    CurrentPosition.Text = t.ToString("m\\:ss");

                    PositionSlider.Maximum = playerService.CurrentTrack.Duration;

                    MaxPosition.Text = playerService.CurrentTrack.DurationString;



                    if (playerService.CurrentTrack.Album != null)
                    {
                        var amim = (Storyboard)(this.Resources["BackgroundAmimate"]);
                        amim.Begin();
                        var bitmapImage = new BitmapImage(new Uri(playerService.CurrentTrack.Album.Cover));
                        TrackCover.ImageSource = bitmapImage;
                        BackgroundCard.Source = bitmapImage;
                    }else
                    {
                        TrackCover.ImageSource = null;
                        BackgroundCard.Source = null;
                    }

                    if (playerService.CurrentTrack.OwnerId == config.UserId)
                    {
                        LikeIcon.Filled = true;

                    }
                    else
                    {
                        LikeIcon.Filled = false;

                    }
                });


                await SaveVolume();
            }
            catch (Exception ex)
            {
                logger.Error("Error in track changed event");
                logger.Error(ex, ex.Message);
                
            }
            
        }

        private void PlayerService_PositionTrackChangedEvent(object? sender, TimeSpan e)
        {
            try
            {
                if (playerService == null) return;

                Application.Current.Dispatcher.BeginInvoke(() =>
                {

                    TimeSpan t = e;
                    if (t.Hours > 0)
                        CurrentPosition.Text = t.ToString("h\\:mm\\:ss");
                    CurrentPosition.Text = t.ToString("m\\:ss");

                    PositionSlider.Value = e.TotalSeconds;
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            
        }

        private void PlayerService_PlayStateChangedEvent(object? sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (playerService == null) return;

                    if (playerService.IsPlaying)
                    {
                        PlayIcon.Symbol = WPFUI.Common.SymbolRegular.Pause24;
                    }
                    else
                    {
                        PlayIcon.Symbol = WPFUI.Common.SymbolRegular.Play32;

                    }
                });
                
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            
        }

        Rect rect = new Rect(0, 0, 0, 100);
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rect.Width = this.ActualWidth;
            rec.Rect = rect;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playerService == null) return;

            playerService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playerService == null) return;

            if (e.NewValue == 0.0)
            {
                SpeakerIcon.Symbol = WPFUI.Common.SymbolRegular.SpeakerOff28;
            }
            else if (e.NewValue > 0.0 && e.NewValue < 0.30)
            {
                SpeakerIcon.Symbol = WPFUI.Common.SymbolRegular.Speaker032;
            }
            else if (e.NewValue > 0.30 && e.NewValue < 0.60)
            {
                SpeakerIcon.Symbol = WPFUI.Common.SymbolRegular.Speaker132;
            }
            else if (e.NewValue > 0.80)
            {
                SpeakerIcon.Symbol = WPFUI.Common.SymbolRegular.Speaker232;
            }


            playerService.SetVolume(e.NewValue);
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerService == null) return;

            await SaveVolume();

            if (playerService.IsPlaying) playerService.Pause();
            else playerService.Play();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerService == null) return;

            await SaveVolume();

            await playerService.NextTrack();
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerService == null) return;

            await playerService.PreviousTrack();
        }

        private void ArtistName_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (playerService.CurrentTrack.MainArtists == null) return;
            ArtistName.TextDecorations.Add(TextDecorations.Underline);
                this.Cursor = Cursors.Hand;

        }

        private void ArtistName_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (playerService.CurrentTrack.MainArtists == null) return;

            foreach (var dec in TextDecorations.Underline)
            {
                ArtistName.TextDecorations.Remove(dec);
            }
            this.Cursor = Cursors.Arrow;

        }

        private async Task SaveVolume()
        {
            var value = Volume.Value * 100;
            var configService = StaticService.Container.Resolve<ConfigService>();

            var conf = await configService.GetConfig();
            conf.Volume = (int)value;


            await configService.SetConfig(conf);
        }

        private async void ArtistName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                if (playerService.CurrentTrack.MainArtists == null)
                {
                    await navigationService.OpenSearchSection(playerService.CurrentTrack.Artist);
                }else
                {
                    await navigationService.OpenArtistSection(playerService.CurrentTrack.MainArtists[0].Id);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        private async void LikeTrack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = StaticService.Container.Resolve<VkService>();
                var notificationService = StaticService.Container.Resolve<Services.NotificationsService>();

                if (playerService.CurrentTrack.OwnerId == config.UserId)
                {

                    LikeIcon.Filled = false;
                    await vkService.AudioDeleteAsync(playerService.CurrentTrack.Id, playerService.CurrentTrack.OwnerId);
                    notificationService.Show("Удалено из вашей библиотеки", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь удален из вашей музыки");
                    playerService.CurrentTrack.OwnerId = 0;
                }
                else
                {
                    LikeIcon.Filled = true;
                    await vkService.AudioAddAsync(playerService.CurrentTrack.Id, playerService.CurrentTrack.OwnerId);

                    notificationService.Show("Добавлено в вашу библиотеку", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь находится в Вашей музыке!");
                }
            }
            catch(Exception ex)
            {
                logger.Error("Error in like track");
                logger.Error(ex, ex.Message);

                var notificationService = StaticService.Container.Resolve<NotificationsService>();

                notificationService.Show("Ошибка", $"Мы не смогли добавить этот трек :с");
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var configService = StaticService.Container.Resolve<ConfigService>();

            this.config = await configService.GetConfig();
            
            if(config.Volume == null)
            {
                config.Volume = 100;

                await configService.SetConfig(config);
            }


            var value = (config.Volume.Value / 100D);

            playerService.SetVolume(value);

            Volume.Value = value;
        }

        DispatcherTimer timer = new DispatcherTimer();
        private void ScrollTrackName()
        {
            try
            {
                bool backscroll = false;
                timer.Tick += (ss, ee) =>
                {
                    if (TitleScroll.ScrollableWidth == 0) return;

                    if (backscroll == false)
                    {
                        TitleScroll.ScrollToHorizontalOffset(TitleScroll.HorizontalOffset + 0.6);
                        if (TitleScroll.HorizontalOffset == TitleScroll.ScrollableWidth)
                            backscroll = true;
                    }

                    if (backscroll == true)
                    {
                        TitleScroll.ScrollToHorizontalOffset(TitleScroll.HorizontalOffset - 0.8);
                        if (TitleScroll.HorizontalOffset == 0)
                        {
                            backscroll = false;
                        }
                    }
                };
                timer.Interval = TimeSpan.FromMilliseconds(15);
                timer.Start();
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }

        private void StopScrollTrackName()
        {
            this.timer.Stop();
            TitleScroll.ScrollToHorizontalOffset(0);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            this.playerService.SetShuffle(ShuffleButton.IsChecked.Value);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.playerService.SetRepeat(RepeatButton.IsChecked.Value);

        }

        private async void OpenFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var notificationService = StaticService.Container.Resolve<Services.NotificationsService>();
            var mainWindow = Window.GetWindow(this);

            var win = new FullScreenWindow(logger, playerService, notificationService);

            ShowOnMonitor(win, mainWindow!);
        }

        private void ShowOnMonitor(Window window, Window mainWindow)
        {
            var screen = WpfScreenHelper.Screen.FromWindow(mainWindow);

            window.WindowStyle = WindowStyle.None;
            window.WindowStartupLocation = WindowStartupLocation.Manual;

            window.Left = screen.Bounds.Left / screen.ScaleFactor;
            window.Top = screen.Bounds.Top / screen.ScaleFactor;


            window.Height = screen.Bounds.Height / screen.ScaleFactor;
            window.Width = screen.Bounds.Width / screen.ScaleFactor;

            window.Show();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadButton.IsEnabled = false;
                var downloader = StaticService.Container.Resolve<DownloaderService>();

                await downloader.AddToQueueAsync(playerService.CurrentTrack);
            }catch(FileNotFoundException ex)
            {

                var navigation = StaticService.Container.Resolve<Services.NavigationService>();

                navigation.NavigateToPage(new DownloadsView());
                //go to download page
            }


        }

        private async void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void TrackTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            ScrollTrackName();
            if (playerService.CurrentTrack.Album != null)
            {
                TrackTitle.TextDecorations.Add(TextDecorations.Underline);
                this.Cursor = Cursors.Hand;
            }
           
        }


        private void TrackTitle_MouseLeave(object sender, MouseEventArgs e)
        {
            StopScrollTrackName();
            foreach (var dec in TextDecorations.Underline)
            {
                TrackTitle.TextDecorations.Remove(dec);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void TrackTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (playerService.CurrentTrack.Album != null)
            {
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
                navigationService.NavigateToPage(new PlaylistView(playerService.CurrentTrack.Album.Id, playerService.CurrentTrack.Album.OwnerId, playerService.CurrentTrack.Album.AccessKey));
            }
        }


        private bool mouseEnteredInVolume = false;
        private void Volume_MouseEnter(object sender, MouseEventArgs e)
        {
            this.mouseEnteredInVolume = true;
        }

        private void Volume_MouseLeave(object sender, MouseEventArgs e)
        {
            this.mouseEnteredInVolume = false;
        }

        private void TitleScroll_Loaded(object sender, RoutedEventArgs e)
        {
            //ScrollTrackName();
        }
    }
}
