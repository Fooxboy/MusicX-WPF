using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для RecommendedPlaylistControl.xaml
    /// </summary>
    public partial class RecommendedPlaylistControl : UserControl
    {
        public RecommendedPlaylist Playlist { get; set; }
        public RecommendedPlaylistControl()
        {
            InitializeComponent();
            this.Loaded += RecommendedPlaylistControl_Loaded;
        }

        private void RecommendedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                double value;

                try
                {
                    value = Convert.ToDouble(Playlist.Percentage);

                }catch(Exception ex)
                {
                    value = Convert.ToDouble(Playlist.Percentage.Replace('.', ','));
                }


                value = Math.Round(value, 2);
                Percent.Text = $"{value * 100}%";
                Subtitle.Text = Playlist.PercentageTitle;

                NamePlaylist.Text = Playlist.Playlist.Title;
                OwnerName.Text = Playlist.Playlist.OwnerName;
                var brr = (SolidColorBrush)new BrushConverter().ConvertFrom(Playlist.Color);

                var brrTwo = brr.Clone();


                Color color = brrTwo.Color;
                color.R += 50;
                color.B += 20;
                color.G += 72;

                GradientBackground.GradientStops.Add(new GradientStop() { Color = brr.Color, Offset = 0.0 });
                GradientBackground.GradientStops.Add(new GradientStop() { Color = color, Offset = 1.1 });

                //BackgroundRectangle.Fill = 

                foreach (var audio in Playlist.Audios)
                {
                    MainStackPanel.Children.Add(new TrackControl() { Audio = audio, Width = 284, Margin = new Thickness(0, 5, 0, 0) });
                }
            }catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }
            

        }

        private void BackgroundRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void BackgroundRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void BackgroundRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            navigationService.NavigateToPage(new PlaylistView(Playlist.Playlist.Id,Playlist.Playlist.OwnerId , Playlist.Playlist.AccessKey));
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

                var playerService = StaticService.Container.Resolve<PlayerService>();

                if (!nowPlay)
                {
                    nowPlay = true;

                    Icons.Glyph = WPFUI.Common.Icon.Timer20;
                    var vkService = StaticService.Container.Resolve<VkService>();

                    var audios = await vkService.AudioGetAsync(Playlist.Playlist.Id, Playlist.Playlist.OwnerId, Playlist.Playlist.AccessKey);

                    await playerService.Play(0, audios.Items);

                    Icons.Glyph = WPFUI.Common.Icon.Pause24;

                    nowLoad = false;

                }
                else
                {
                    playerService.Pause();
                    Icons.Glyph = WPFUI.Common.Icon.Play24;

                    await Task.Delay(400);
                    nowLoad = false;

                    nowPlay = false;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                nowLoad = false;
            }
        }
    }
}
