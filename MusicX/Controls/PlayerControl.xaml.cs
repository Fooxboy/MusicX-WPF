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


        private List<Listener> connectedListeners;
        private readonly PlayerService playerService;
        private readonly ListenTogetherService listenTogetherService;
        private readonly Logger logger;
        private ConfigModel config;
        private Dictionary<int, Border> UserBorderAvatars;
        private Dictionary<int, ImageBrush> UserAvatars;

        public PlayerControl()
        {
            InitializeComponent();

            this.playerService = StaticService.Container.GetRequiredService<PlayerService>();
            this.listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            this.logger = StaticService.Container.GetRequiredService<Logger>();
            playerService.PlayStateChangedEvent += PlayerService_PlayStateChangedEvent;
            playerService.PositionTrackChangedEvent += PlayerService_PositionTrackChangedEvent;
            playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
            playerService.QueueLoadingStateChanged += PlayerService_QueueLoadingStateChanged;
            playerService.TrackLoadingStateChanged += PlayerService_TrackLoadingStateChanged;

            listenTogetherService.ConnectedToSession += ListenTogetherConnectedToSession;
            listenTogetherService.StartedSession += ListenTogetherStartedSession;
            listenTogetherService.SessionOwnerStoped += ListenTogetherStopedSession;
            listenTogetherService.SessionStoped += ListenTogetherSessionStoped;
            listenTogetherService.LeaveSession += ListenTogetherStopedSession;
            listenTogetherService.ListenerConnected += ListenTogetherListenerConnected;
            listenTogetherService.ListenerDisconnected += ListenTogetherServiceListenerDisconnected;
            this.MouseWheel += PlayerControl_MouseWheel;
            
            Queue.ItemsSource = playerService.Tracks;

            UserBorderAvatars = new Dictionary<int, Border>()
            {
                {1, UserBorderAvatar1},
                {2, UserBorderAvatar2},
                {3, UserBorderAvatar3},
            };

            UserAvatars = new Dictionary<int, ImageBrush>()
            {
                {1, UserAvatar1 },
                {2, UserAvatar2 },
                {3, UserAvatar3 },
            };

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

        Rect rect = new Rect(0, 0, 0, 100);
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rect.Width = this.ActualWidth;
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

            var conf = await configService.GetConfig();
            conf.Volume = (int)value;
            conf.IsMuted = playerService.IsMuted;


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
                        LikeIcon.Filled = false;
                        await vkService.AudioDeleteAsync(data.Info.Id, data.Info.OwnerId);
                        notificationService.Show("Удалено из вашей библиотеки", $"Трек {this.ArtistName.Text} - {this.TrackTitle.Text} теперь удален из вашей музыки");
                        break;
                    case VkTrackData data:
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

            this.config = await configService.GetConfig();
            
            if(config.Volume == null)
            {
                config.Volume = 100;

                await configService.SetConfig(config);
            }


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

        private async void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
           
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

        /// <summary>
        /// Пользователь подключился как слушатель
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task ListenTogetherConnectedToSession(PlaylistTrack playlistTrack)
        {
            try
            {
                ButtonsStackPanel.Visibility = Visibility.Collapsed;
                DownloadButton.Visibility = Visibility.Collapsed;
                QueueButton.Visibility = Visibility.Collapsed;

                ListenTogether.Visibility = Visibility.Visible;
                Owner.Visibility = Visibility.Collapsed;
                Listener.Visibility = Visibility.Visible;

                ListenTogetherBar.Width = new GridLength(340);

                var vkService = StaticService.Container.GetRequiredService<VkService>();

                var ownerInfo = await listenTogetherService.GetOwnerSessionInfoAsync();

                var owner = await vkService.GetUserAsync(ownerInfo.VkId);

                OwnerAvatar.ImageSource = new BitmapImage(owner.Photo200);
                OwnerName.Text = $"{owner.FirstName} {owner.LastName}";

            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");

                logger.Error(ex, ex.Message);

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

        /// <summary>
        /// Пользователь оставил сессию
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task ListenTogetherStopedSession()
        {
            try
            {
                ButtonsStackPanel.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Visible;
                QueueButton.Visibility = Visibility.Visible;

                ListenTogetherBar.Width = new GridLength(0);
                if (connectedListeners != null) connectedListeners.Clear();
                ListenTogether.Visibility = Visibility.Collapsed;
                Owner.Visibility = Visibility.Collapsed;
                Listener.Visibility = Visibility.Collapsed;
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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


        /// <summary>
        /// Владелец запустил сессию
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task ListenTogetherStartedSession()
        {
            try
            {
                ListenTogetherBar.Width = new GridLength(240);
                connectedListeners = new List<Listener>();
                ListenTogether.Visibility = Visibility.Visible;
                Owner.Visibility = Visibility.Visible;
                CountListeners.Text = connectedListeners.Count.ToString();
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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
         

        /// <summary>
        /// Владелец остановил сессию.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task ListenTogetherSessionStoped()
        {
            try
            {
                this.ListenTogetherStart.Visibility = Visibility.Visible;
                ListenTogetherBar.Width = new GridLength(0);
                connectedListeners.Clear();
                ListenTogether.Visibility = Visibility.Collapsed;
                Owner.Visibility = Visibility.Collapsed;
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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


        private async Task ListenTogetherListenerConnected(User listener)
        {
            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();

                var user = await vkService.GetUserAsync(listener.VkId);

                connectedListeners.Add(new Listener() { Ids = listener, Name = user.FirstName + " " + user.LastName, Photo = user.Photo200.ToString() });
                CountListeners.Text = connectedListeners.Count.ToString();

                if (UserBorderAvatars.TryGetValue(connectedListeners.Count, out var border))
                {
                    border.Visibility = Visibility.Visible;
                }

                if (UserAvatars.TryGetValue(connectedListeners.Count, out var avatar))
                {
                    avatar.ImageSource = new BitmapImage(user.Photo200);
                }
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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

        private async Task ListenTogetherServiceListenerDisconnected(User listr)
        {
            try
            {
                var listener = connectedListeners.SingleOrDefault(l => l.Ids.VkId == listr.VkId);

                if (listener is null) return;
                var position = connectedListeners.IndexOf(listener);
                connectedListeners.Remove(listener);
                CountListeners.Text = connectedListeners.Count.ToString();

                for (var i = 0; i < connectedListeners.Count; i++)
                {
                    var usr = connectedListeners[i];
                    if (UserBorderAvatars.TryGetValue(i + 1, out var border))
                    {
                        border.Visibility = Visibility.Visible;
                    }

                    if (UserAvatars.TryGetValue(i + 1, out var avatar))
                    {
                        avatar.ImageSource = new BitmapImage(new(usr.Photo));
                    }
                }

                if (connectedListeners.Count < 3)
                {
                    if (UserBorderAvatars.TryGetValue(connectedListeners.Count + 1, out var border))
                    {
                        border.Visibility = Visibility.Collapsed;
                    }
                }
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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

        private async void ListenTogetherStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ListenTogetherStart.Visibility = Visibility.Collapsed;

                var configService = StaticService.Container.GetRequiredService<ConfigService>();

                var config = await configService.GetConfig();

                await listenTogetherService.StartSessionAsync(config.UserId);

                await listenTogetherService.ChangeTrackAsync(this.playerService.CurrentTrack);
            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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

        private async void StopSession(object sender, RoutedEventArgs e)
        {
            try
            {
                await listenTogetherService.StopPlaySessionAsync();

            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка остановки сессии", "При остановке сессии произошла ошибка");
                logger.Error(ex, ex.Message);

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

        private async void DisconnectSession(object sender, RoutedEventArgs e)
        {
            try
            {
                await listenTogetherService.LeavePlaySessionAsync();

            }catch(Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

                notificationsService.Show("Ошибка отключения от сессии", "При отключении от сессии произошла ошибка");

                logger.Error(ex, ex.Message);

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
    }
}
