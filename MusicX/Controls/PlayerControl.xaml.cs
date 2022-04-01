using DryIoc;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            
        }

        private void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            try
            {
                if (playerService == null) return;

                Application.Current.Dispatcher.BeginInvoke(() =>
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
                    }

                    if (playerService.CurrentTrack.OwnerId == config.UserId)
                    {
                        LikeIcon.Glyph = '\uE00B';
                    }
                    else
                    {
                        LikeIcon.Glyph = '\uE006';

                    }
                });
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
                        PlayIcon.Glyph = '\uE103';

                    }
                    else
                    {
                        PlayIcon.Glyph = '\uE102';
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
                SpeakerIcon.Glyph = WPFUI.Common.Icon.SpeakerOff28;
            }
            else if (e.NewValue > 0.0 && e.NewValue < 0.50)
            {
                SpeakerIcon.Glyph = WPFUI.Common.Icon.Speaker048;
            }
            else if (e.NewValue > 0.50 && e.NewValue < 0.60)
            {
                SpeakerIcon.Glyph = WPFUI.Common.Icon.Speaker124;
            }
            else if (e.NewValue > 0.60 && e.NewValue < 0.90)
            {
                SpeakerIcon.Glyph = WPFUI.Common.Icon.Speaker216;

            }
            else if (e.NewValue > 0.90)
            {
                SpeakerIcon.Glyph = WPFUI.Common.Icon.Speaker248;
            }


            playerService.SetVolume(e.NewValue);
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerService == null) return;

            if (playerService.IsPlaying) playerService.Pause();
            else playerService.Play();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerService == null) return;

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

                if (playerService.CurrentTrack.OwnerId == config.UserId)
                {
                    LikeIcon.Glyph = '\uE006';
                    await vkService.AudioDeleteAsync(playerService.CurrentTrack.Id, playerService.CurrentTrack.OwnerId);

                }
                else
                {
                    LikeIcon.Glyph = '\uE00B';
                    await vkService.AudioAddAsync(playerService.CurrentTrack.Id, playerService.CurrentTrack.OwnerId);
                }
            }catch(Exception ex)
            {
                logger.Error("Error in like track");
                logger.Error(ex, ex.Message);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var configService = StaticService.Container.Resolve<ConfigService>();

            this.config = await configService.GetConfig();

        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ;

            this.playerService.SetShuffle(ShuffleButton.IsChecked.Value);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.playerService.SetRepeat(RepeatButton.IsChecked.Value);

        }

        private void OpenFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var win = new FullScreenWindow(logger, playerService);
            win.Show();
        }
    }
}
