using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Behaviors;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels;
using Wpf.Ui.Common;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using MusicX.Shared.Player;
using MusicX.Shared.ListenTogether;
using System.Windows.Media;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public static readonly DependencyProperty IsTrackLoadingProperty = DependencyProperty.Register(
            nameof(IsTrackLoading), typeof(bool), typeof(PlayerControl));

        public bool IsTrackLoading
        {
            get => (bool)GetValue(IsTrackLoadingProperty);
            set => SetValue(IsTrackLoadingProperty, value);
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            nameof(IsPlaying), typeof(bool), typeof(PlayerControl));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }
        
        private readonly PlayerService playerService;
        private readonly ListenTogetherService listenTogetherService;
        private readonly Logger logger;
        private ConfigModel config;

        public PlayerControl()
        {
            InitializeComponent();

            this.playerService = StaticService.Container.GetRequiredService<PlayerService>();
            this.logger = StaticService.Container.GetRequiredService<Logger>();
            this.listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            playerService.PlayStateChangedEvent += PlayerService_PlayStateChangedEvent;
            playerService.PositionTrackChangedEvent += PlayerService_PositionTrackChangedEvent;
            playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
            playerService.QueueLoadingStateChanged += PlayerService_QueueLoadingStateChanged;
            playerService.TrackLoadingStateChanged += PlayerService_TrackLoadingStateChanged;
            listenTogetherService.ConnectedToSession += ListenTogetherService_ConnectedToSession;
            listenTogetherService.LeaveSession += ListenTogetherService_LeaveSession;
            listenTogetherService.SessionOwnerStoped += ListenTogetherService_LeaveSession;
            this.MouseWheel += PlayerControl_MouseWheel;
            
            Queue.ItemsSource = playerService.Tracks;
        }

        private Task ListenTogetherService_LeaveSession()
        {
            ButtonsStackPanel.Visibility = Visibility.Visible;
            QueueButton.Visibility = Visibility.Visible;
            return Task.CompletedTask;
        }

        private Task ListenTogetherService_ConnectedToSession(PlaylistTrack arg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ButtonsStackPanel.Visibility = Visibility.Collapsed;
                QueueButton.Visibility = Visibility.Collapsed;
            });

            return Task.CompletedTask;
        }

        private void PlayerService_TrackLoadingStateChanged(object? sender, PlayerLoadingEventArgs e)
        {
            IsTrackLoading = e.State is PlayerLoadingState.Started;
        }

        private void PlayerService_QueueLoadingStateChanged(object? sender, PlayerLoadingEventArgs e)
        {
            QueueLoadingRing.Visibility = e.State == PlayerLoadingState.Started ? Visibility.Visible : Visibility.Collapsed;
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
                DataContext = playerService.CurrentTrack;
                if (playerService.CurrentTrack!.Data.IsExplicit)
                {
                    explicitBadge.Visibility = Visibility.Visible;

                }
                else
                {
                    explicitBadge.Visibility = Visibility.Collapsed;

                }

                TrackTitle.Text = playerService.CurrentTrack.Title;
                string s = string.Empty;
                if (playerService.CurrentTrack!.MainArtists.Any())
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
                    ArtistName.Text = playerService.CurrentTrack.GetArtistsString();
                }


                TimeSpan t = playerService.Position;
                if (t.Hours > 0)
                    CurrentPosition.Text = t.ToString("h\\:mm\\:ss");
                CurrentPosition.Text = t.ToString("m\\:ss");

                PositionSlider.Maximum = playerService.CurrentTrack.Data.Duration.TotalSeconds;

                MaxPosition.Text = playerService.CurrentTrack.Data.Duration.ToString("m\\:ss");



                if (playerService.CurrentTrack.AlbumId != null)
                {
                    var amim = (Storyboard)(this.Resources["BackgroundAmimate"]);
                    amim.Begin();
                    var bitmapImage = new BitmapImage(new Uri(playerService.CurrentTrack.AlbumId.CoverUrl));
                    TrackCover.ImageSource = bitmapImage;
                    BackgroundCard.ImageSource = bitmapImage;
                }else
                {
                    TrackCover.ImageSource = null;
                    BackgroundCard.ImageSource = null;
                }

                LikeIcon.Filled = playerService.CurrentTrack.Data.IsLiked;
                DownloadButton.IsEnabled = true;
                Queue.ScrollIntoView(playerService.CurrentTrack);


                await SaveVolume();
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
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
            }
            
        }

        private void PlayerService_PlayStateChangedEvent(object? sender, EventArgs e)
        {
            IsPlaying = playerService.IsPlaying;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playerService == null || !PositionSlider.IsMouseOver) return;

            playerService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playerService == null) return;
            playerService.SetVolume(e.NewValue);
            playerService.IsMuted = false;
            UpdateSpeakerIcon();
        }

        private void UpdateSpeakerIcon()
        {
            SpeakerIcon.Icon = playerService.Volume switch
            {
                _ when playerService.IsMuted => SymbolRegular.SpeakerOff28,
                0.0 => SymbolRegular.SpeakerOff28,
                > 0.0 and < 0.30 => SymbolRegular.Speaker032,
                > 0.30 and < 0.60 => SymbolRegular.Speaker132,
                > 0.80 => SymbolRegular.Speaker232,
                _ => SpeakerIcon.Icon
            };
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
            var configService = StaticService.Container.GetRequiredService<ConfigService>();
            var windowsAudioService = StaticService.Container.GetRequiredService<WindowsAudioMixerService>();

            var conf = await configService.GetConfig();
            conf.Volume = (int)value;
            conf.IsMuted = playerService.IsMuted;

            var mixerVolume = windowsAudioService.GetVolume();
            conf.MixerVolume= (int)mixerVolume;

            await configService.SetConfig(conf);
        }

        private async void ArtistName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                if (playerService.CurrentTrack?.MainArtists.First().Id is {Type: ArtistIdType.Vk} artistId)
                {
                    navigationService.OpenSection(artistId.Id, SectionType.Artist);
                }
                else
                {
                    navigationService.OpenSection(playerService.CurrentTrack!.GetArtistsString(), SectionType.Search);
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

                logger.Error(ex, ex.Message);
            }

        }

        private async void LikeTrack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();
                var notificationService = StaticService.Container.GetRequiredService<Services.NotificationsService>();

                switch (playerService.CurrentTrack?.Data)
                {
                    case VkTrackData {IsLiked: true} data:
                        if(!LikeIcon.Filled)
                        {
                            LikeIcon.Filled = true;
                            await vkService.AudioAddAsync(data.Info.Id, data.Info.OwnerId);

                            notificationService.Show("Добавлено в вашу библиотеку", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь находится в Вашей музыке!");
                            break;
                        }

                        LikeIcon.Filled = false;
                        await vkService.AudioDeleteAsync(data.Info.Id, data.Info.OwnerId);
                        notificationService.Show("Удалено из вашей библиотеки", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь удален из вашей музыки");
                        break;
                    case VkTrackData data:
                        if (LikeIcon.Filled)
                        {
                            LikeIcon.Filled = false;
                            await vkService.AudioDeleteAsync(data.Info.Id, data.Info.OwnerId);
                            notificationService.Show("Удалено из вашей библиотеки", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь удален из вашей музыки");
                            break;
                        }

                        LikeIcon.Filled = true;
                        await vkService.AudioAddAsync(data.Info.Id, data.Info.OwnerId);

                        notificationService.Show("Добавлено в вашу библиотеку", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь находится в Вашей музыке!");
                        break;
                }
            }
            catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Error in like track");
                logger.Error(ex, ex.Message);

                var notificationService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationService.Show("Ошибка", $"Мы не смогли добавить этот трек :с");
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var configService = StaticService.Container.GetRequiredService<ConfigService>();
            var mixerService = StaticService.Container.GetRequiredService<WindowsAudioMixerService>();

            this.config = await configService.GetConfig();
            
            if(config.Volume == null)
            {
                config.Volume = 100;

                await configService.SetConfig(config);
            }

            if (config.MixerVolume is null)
            {
                config.MixerVolume = 100;
                await configService.SetConfig(config);
            }

            mixerService.SetVolume((float)config.MixerVolume);
            var value = (config.Volume.Value / 100D);

            playerService.SetVolume(value);
            
            Volume.Value = value;
            playerService.IsMuted = config.IsMuted;
            UpdateSpeakerIcon();
        }

        private void StopScrollTrackName()
        {
            AutoScrollBehavior.GetController(TitleScroll)?.Pause();
            TitleScroll.ScrollToHorizontalOffset(0);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            this.playerService.SetShuffle(true);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.playerService.SetRepeat(RepeatButton.IsChecked.Value);

        }

        private async void OpenFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var notificationService = StaticService.Container.GetRequiredService<Services.NotificationsService>();
            var mainWindow = Window.GetWindow(this);

            if (fullScreenWindow is not null || mainWindow is null)
                return;
            fullScreenWindow = new FullScreenWindow(logger, playerService, notificationService);

            ShowOnMonitor(fullScreenWindow, mainWindow);
            fullScreenWindow.Closed += FullScreenWindowOnClosed;
        }
        private void FullScreenWindowOnClosed(object? sender, EventArgs e)
        {
            fullScreenWindow = null;
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
                var downloader = StaticService.Container.GetRequiredService<DownloaderViewModel>();

                downloader.DownloadQueue.Add(playerService.CurrentTrack!);
                downloader.StartDownloadingCommand.Execute(null);
            }catch(FileNotFoundException ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var navigation = StaticService.Container.GetRequiredService<Services.NavigationService>();

                navigation.OpenMenuSection("downloads");
                //go to download page
            }


        }

        private void TrackTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (AutoScrollBehavior.GetController(TitleScroll) is { } controller)
                controller.Play();
            else
                AutoScrollBehavior.SetAutoScroll(TitleScroll, true);
            
            if (playerService.CurrentTrack!.AlbumId != null)
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
            if (playerService.CurrentTrack?.AlbumId is VkAlbumId albumId)
            {
                var (id, ownerId, accessKey, _, _) = albumId;
                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
                navigationService.OpenExternalPage(new PlaylistView(id, ownerId, accessKey));
            }
        }


        private bool mouseEnteredInVolume = false;
        private FullScreenWindow? fullScreenWindow;
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
        private void DeleteFromQueue_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement {DataContext: PlaylistTrack audio})
                playerService.RemoveFromQueue(audio);
        }
        private void ReorderButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement {TemplatedParent: FrameworkElement {TemplatedParent: ListBoxItem item}})
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
        }
        private void ListBoxItem_OnDrop(object sender, DragEventArgs e)
        {
            var source = (PlaylistTrack)e.Data.GetData(typeof(PlaylistTrack))!;
            var target = (PlaylistTrack)((ListBoxItem)sender).DataContext;

            var list = playerService.Tracks;
            
            var removedIdx = list.IndexOf(source);
            var targetIdx = list.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                list.Insert(targetIdx + 1, source);
                list.RemoveAt(removedIdx);
            }
            else
            {
                if (list.Count + 1 <= ++removedIdx)
                    return;
                list.Insert(targetIdx, source);
                list.RemoveAt(removedIdx);
            }

            var currentIndex = list.IndexOf(playerService.CurrentTrack);
            
            // insert index is next track
            if (currentIndex + 1 == targetIdx)
                playerService.NextPlayTrack = source;
            else if (currentIndex + 1 < list.Count)
                playerService.NextPlayTrack = list[currentIndex + 1];

            playerService.CurrentIndex = currentIndex;
        }
        private void SpeakerIcon_OnClick(object sender, RoutedEventArgs e)
        {
            playerService.IsMuted = !playerService.IsMuted;
            UpdateSpeakerIcon();
        }
    }
}
