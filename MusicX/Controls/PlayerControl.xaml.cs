using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Behaviors;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Models;
using MusicX.Patches;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using MusicX.Views;
using MusicX.Views.Modals;
using NLog;
using ProtoBuf.Meta;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using WpfScreenHelper;
using NavigationService = MusicX.Services.NavigationService;

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

        public static readonly DependencyProperty TracksSourceProperty = DependencyProperty.Register(
            nameof(TracksSource), typeof(IEnumerable), typeof(PlayerControl), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable TracksSource
        {
            get => (IEnumerable)GetValue(TracksSourceProperty);
            set => SetValue(TracksSourceProperty, value);
        }

        public PlayerService PlayerService { get; }
        
        private readonly ListenTogetherService listenTogetherService;
        private readonly Logger logger;
        private ConfigModel config;

        public PlayerControl()
        {
            InitializeComponent();

            this.PlayerService = StaticService.Container.GetRequiredService<PlayerService>();
            this.logger = StaticService.Container.GetRequiredService<Logger>();
            this.listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            PlayerService.PlayStateChangedEvent += PlayerService_PlayStateChangedEvent;
            PlayerService.PositionTrackChangedEvent += PlayerService_PositionTrackChangedEvent;
            PlayerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
            PlayerService.QueueLoadingStateChanged += PlayerService_QueueLoadingStateChanged;
            PlayerService.TrackLoadingStateChanged += PlayerService_TrackLoadingStateChanged;
            PlayerService.CurrentPlaylistChanged += PlayerService_CurrentPlaylistChanged;
            listenTogetherService.ConnectedToSession += ListenTogetherService_ConnectedToSession;
            listenTogetherService.LeaveSession += ListenTogetherService_LeaveSession;
            listenTogetherService.SessionOwnerStoped += ListenTogetherService_LeaveSession;
            this.MouseWheel += PlayerControl_MouseWheel;
        }

        private void PlayerService_CurrentPlaylistChanged(object? sender, EventArgs e)
        {
            ShuffleButton.IsChecked = false;
            Queue.ItemsSource = PlayerService.Tracks;
        }

        private Task ListenTogetherService_LeaveSession()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ButtonsStackPanel.Visibility = Visibility.Visible;
                QueueButton.Visibility = Visibility.Visible;
            });
           
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

            if (PlayerService == null) return;

            var delta = e.Delta/1000d;
            Volume.Value += delta;
        }

        private async void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            try
            {

                if (PlayerService == null) return;
                DataContext = PlayerService.CurrentTrack;
                if (PlayerService.CurrentTrack!.Data.IsExplicit)
                {
                    explicitBadge.Visibility = Visibility.Visible;

                }
                else
                {
                    explicitBadge.Visibility = Visibility.Collapsed;

                }

                TrackTitle.Text = PlayerService.CurrentTrack.Title;
                
                if (ArtistName.Inlines.Count > 0)
                    ArtistName.Inlines.Clear();
                
                if (PlayerService.CurrentTrack!.MainArtists.Any())
                {
                    ArtistName.Inlines.AddRange(PlayerService.CurrentTrack.MainArtists.Select(b =>
                    {
                        var run = new Run(b.Name + ", ")
                        {
                            Tag = b
                        };
                        
                        run.MouseEnter += ArtistName_MouseEnter;
                        run.MouseLeave += ArtistName_MouseLeave;
                        run.MouseLeftButtonUp += ArtistName_MouseLeftButtonUp;
                            
                        return run;
                    }));

                    var lastInline = (Run)ArtistName.Inlines.LastInline;
                    lastInline.Text = lastInline.Text[..^2];
                }
                else
                {
                    var run = new Run(PlayerService.CurrentTrack.GetArtistsString());
                            
                    run.MouseEnter += ArtistName_MouseEnter;
                    run.MouseLeave += ArtistName_MouseLeave;
                    run.MouseLeftButtonUp += ArtistName_MouseLeftButtonUp;
                    
                    ArtistName.Inlines.Add(run);
                }


                TimeSpan t = PlayerService.Position;
                if (t.Hours > 0)
                    CurrentPosition.Text = t.ToString("h\\:mm\\:ss");
                CurrentPosition.Text = t.ToString("m\\:ss");

                PositionSlider.Maximum = PlayerService.CurrentTrack.Data.Duration.TotalSeconds;

                MaxPosition.Text = PlayerService.CurrentTrack.Data.Duration.ToString("m\\:ss");



                if (PlayerService.CurrentTrack.AlbumId?.CoverUrl != null)
                {
                    var amim = (Storyboard)(this.Resources["BackgroundAmimate"]);
                    amim.Begin();
                    var bitmapImage = new BitmapImage(new Uri(PlayerService.CurrentTrack.AlbumId.CoverUrl));
                    TrackCover.ImageSource = bitmapImage;
                    BackgroundCard.ImageSource = bitmapImage;
                }else
                {
                    TrackCover.ImageSource = null;
                    BackgroundCard.ImageSource = null;
                }

                LikeIcon.Filled = PlayerService.CurrentTrack.Data.IsLiked;
                DownloadButton.IsEnabled = true;
                Queue.ScrollIntoView(PlayerService.CurrentTrack);

                switch (PlayerService.CurrentTrack?.Data)
                {
                    case VkTrackData vkData when LikeIcon.Filled:
                        DislikeButton.Visibility = Visibility.Collapsed;
                        break;
                    case VkTrackData vkData:
                        DislikeButton.Visibility = Visibility.Visible;
                        break;
                    case BoomTrackData boomData:
                        DislikeButton.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        DislikeButton.Visibility = Visibility.Collapsed;
                        break;
                }

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
                if (PlayerService == null) return;

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
            IsPlaying = PlayerService.IsPlaying;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PlayerService == null || !PositionSlider.IsMouseOver) return;

            PlayerService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PlayerService == null) return;
            PlayerService.SetVolume(e.NewValue);
            PlayerService.IsMuted = false;
            UpdateSpeakerIcon();
        }

        private void UpdateSpeakerIcon()
        {
            SpeakerIcon.Icon = PlayerService.Volume switch
            {
                _ when PlayerService.IsMuted => new SymbolIcon(SymbolRegular.SpeakerOff28),
                0.0 => new SymbolIcon(SymbolRegular.SpeakerOff28),
                > 0.0 and < 0.10 => new SymbolIcon(SymbolRegular.Speaker032),
                > 0.10 and < 0.60 => new SymbolIcon(SymbolRegular.Speaker132),
                > 0.80 => new SymbolIcon(SymbolRegular.Speaker232),
                _ => SpeakerIcon.Icon
            };
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService == null) return;

            await SaveVolume();

            if (PlayerService.IsPlaying) PlayerService.Pause();
            else PlayerService.Play();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService == null) return;

            await SaveVolume();

            await PlayerService.NextTrack();
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService == null) return;

            await PlayerService.PreviousTrack();
        }

        private void ArtistName_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Run)sender).TextDecorations.Add(TextDecorations.Underline);
            this.Cursor = Cursors.Hand;
        }

        private void ArtistName_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var dec in TextDecorations.Underline)
            {
                ((Run)sender).TextDecorations.Remove(dec);
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
            conf.IsMuted = PlayerService.IsMuted;

            var mixerVolume = windowsAudioService.GetVolume();
            if (mixerVolume.HasValue)
                conf.MixerVolume= (int)mixerVolume;

            await configService.SetConfig(conf);
        }

        private async void ArtistName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

                if (((Run)sender).Tag is TrackArtist { Id: { Type: ArtistIdType.Vk } artistId })
                {
                    navigationService.OpenSection(artistId.Id, SectionType.Artist);
                }
                else
                {
                    navigationService.OpenSection(PlayerService.CurrentTrack!.GetArtistsString(), SectionType.Search);
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
                var boomService = StaticService.Container.GetRequiredService<BoomService>();
                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
                
                switch (PlayerService.CurrentTrack?.Data)
                {
                    case VkTrackData vkData when LikeIcon.Filled:
                        await vkService.Dislike(vkData.Info.Id, vkData.Info.OwnerId);
                        await vkService.AudioDeleteAsync(vkData.Info.Id, vkData.Info.OwnerId);
                        break;
                    case VkTrackData vkData:
                        await vkService.AudioAddAsync(vkData.Info.Id, vkData.Info.OwnerId);
                        break;
                    case BoomTrackData boomData when LikeIcon.Filled:
                        await boomService.UnLike(boomData.Id);
                        break;
                    case BoomTrackData boomData:
                        await boomService.Like(boomData.Id);
                        break;
                    default:
                        return;
                }

                if(!LikeIcon.Filled)
                {
                    LikeIcon.Filled = true;

                    snackbarService.Show("Добавлено в вашу библиотеку",
                        $"Трек {PlayerService.CurrentTrack.GetArtistsString()} - {PlayerService.CurrentTrack.Title} теперь находится в Вашей музыке!", ControlAppearance.Success);
                    return;
                }

                LikeIcon.Filled = false;
                snackbarService.Show("Удалено из вашей библиотеки",
                    $"Трек {PlayerService.CurrentTrack.GetArtistsString()} - {PlayerService.CurrentTrack.Title} теперь удален из вашей музыки", ControlAppearance.Success);
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

                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.ShowException("Мы не смогли добавить этот трек :с", ex);
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

            PlayerService.SetVolume(value);
            
            Volume.Value = value;
            PlayerService.IsMuted = config.IsMuted;
            UpdateSpeakerIcon();
        }

        private void StopScrollTrackName()
        {
            AutoScrollBehavior.GetController(TitleScroll)?.Pause();
            TitleScroll.ScrollToHorizontalOffset(0);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayerService.SetShuffle(ShuffleButton.IsChecked.Value);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayerService.SetRepeat(RepeatButton.IsChecked.Value);

        }

        private void OpenFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            var mainWindow = Window.GetWindow(this);

            if (fullScreenWindow is not null || mainWindow is null)
                return;
            fullScreenWindow = new(logger, PlayerService, snackbarService);

            ShowOnMonitor(fullScreenWindow, mainWindow);
            fullScreenWindow.Closed += FullScreenWindowOnClosed;
        }
        private void FullScreenWindowOnClosed(object? sender, EventArgs e)
        {
            fullScreenWindow = null;
        }

        private void ShowOnMonitor(Window window, Window mainWindow)
        {
            var screen = Screen.FromWindow(mainWindow);

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

                downloader.DownloadQueue.Add(PlayerService.CurrentTrack!);
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

                var navigation = StaticService.Container.GetRequiredService<NavigationService>();

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
            
            if (PlayerService.CurrentTrack!.AlbumId != null)
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
            if (PlayerService.CurrentTrack?.AlbumId is VkAlbumId albumId)
            {
                var (id, ownerId, accessKey, _, _, _) = albumId;
                var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
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
            if (sender is FrameworkElement {TemplatedParent: FrameworkElement {TemplatedParent: ListBoxItem item}})
                PlayerService.RemoveFromQueue(ItemContainerGeneratorIndexHook.GetItemContainerIndex(item));
        }
        private void ReorderButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement {TemplatedParent: FrameworkElement {TemplatedParent: ListBoxItem item}})
                DragDrop.DoDragDrop(item, ItemContainerGeneratorIndexHook.GetItemContainerIndex(item), DragDropEffects.Move);
        }
        
        private void ListBoxItem_OnDrop(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement item)
                return;
            
            var oldIndex = (int)e.Data.GetData(typeof(int))!;
            var newIndex = ItemContainerGeneratorIndexHook.GetItemContainerIndex(item);

            PlayerService.MoveTrackInQueue(oldIndex, newIndex);
        }
        private void SpeakerIcon_OnClick(object sender, RoutedEventArgs e)
        {
            PlayerService.IsMuted = !PlayerService.IsMuted;
            UpdateSpeakerIcon();
        }

        private void TextTrack_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            var lyricsViewModel = StaticService.Container.GetRequiredService<LyricsViewModel>();
            lyricsViewModel.Track = PlayerService.CurrentTrack;

            navigationService.OpenModal<LyricsModal>(lyricsViewModel);
        }

        private async void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();

                if (PlayerService.CurrentTrack?.Data is VkTrackData vkData)
                {
                    await vkService.Dislike(vkData.Info.Id, vkData.Info.OwnerId);
                }

                await PlayerService.NextTrack();
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Error in dislike track");
                logger.Error(ex, ex.Message);

                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.ShowException("Мы не смогли указать, что Вам этот трек не нравится", ex);
            }
        }
    }
}
