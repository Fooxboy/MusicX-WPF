using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views.Modals;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        private readonly Logger logger;

        public BitmapImage BitImage { get; set; }

        public TrackControl()
        {
            InitializeComponent();
            logger = StaticService.Container.Resolve<Logger>();

            this.Unloaded += TrackControl_Unloaded;
          

        }

        private void TrackControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Cover.ImageSource = null;
            this.Audio = null;

        }

        public static readonly DependencyProperty ShowCardProperty =
            DependencyProperty.Register("ShowCard", typeof(bool), typeof(TrackControl), new PropertyMetadata(true));

        public bool ShowCard
        {
            get { return (bool)GetValue(ShowCardProperty); }
            set
            {
                SetValue(ShowCardProperty, value);
            }
        }

        public static readonly DependencyProperty ChartPositionProperty =
            DependencyProperty.Register("ChartPosition", typeof(int), typeof(TrackControl), new PropertyMetadata(0));

        public int ChartPosition
        {
            get { return (int)GetValue(ChartPositionProperty); }
            set
            {
                SetValue(ChartPositionProperty, value);
            }
        }

        public static readonly DependencyProperty AudioProperty =
          DependencyProperty.Register("Audio", typeof(Audio), typeof(TrackControl), new PropertyMetadata(new Audio()));

        public Audio Audio
        {
            get { return (Audio)GetValue(AudioProperty); }
            set
            {
                SetValue(AudioProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShowCard) this.Card.Visibility = Visibility.Visible;
                else this.Card.Visibility = Visibility.Collapsed;

                Title.Text = Audio.Title;
                Subtitle.Text = Audio.Subtitle;
                if (ChartPosition != 0)
                {
                    ChartGrid.Visibility = Visibility.Visible;
                    ChartPositionText.Text = ChartPosition.ToString();
                }

                if(!Audio.IsAvailable || Audio.Url == String.Empty)
                {
                    Title.Text = Audio.Title;
                    Subtitle.Text = Audio.Subtitle;
                    Artists.Text = Audio.Artist;
                    this.Opacity = 0.3;
                    return;
                }

              
                if(BitImage != null)
                {
                    Cover.ImageSource = BitImage;
                }else
                {
                    if (Audio.Album != null)
                    {
                        if (Audio.Album.Cover != null) Cover.ImageSource = new BitmapImage(new Uri(Audio.Album.Cover)) { DecodePixelHeight = 45, DecodePixelWidth = 45, CacheOption = BitmapCacheOption.None };

                    }
                }

                var time = string.Empty;
                TimeSpan t = TimeSpan.FromSeconds(Audio.Duration);
                if (t.Hours > 0) time = t.ToString("h\\:mm\\:ss");
                time = t.ToString("m\\:ss");


                Time.Text = time;

                if (Audio.MainArtists?.Count > 0)
                {
                    string s = string.Empty;
                    foreach (var trackArtist in Audio.MainArtists)
                    {
                        s += trackArtist.Name + ", ";
                    }

                    var artists = s.Remove(s.Length - 2);

                    Artists.Text = artists;

                }
                else
                {
                    Artists.Text = Audio.Artist;
                }
            }catch(Exception ex)
            {
                logger.Error("Failed load track control");
                logger.Error(ex, ex.Message);

                Title.Text = "Невозможно загрузить";
                Subtitle.Text = "это аудио";

                Artists.Text = "Попробуйте позже";
            }
            
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                PlayButtons.Visibility = Visibility.Visible;
                if (!ShowCard)
                {
                    Card.Visibility = Visibility.Visible;

                }
                Card.Opacity = 0.5;
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

            }

        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (Card == null) return;
                PlayButtons.Visibility = Visibility.Collapsed;

                if (!ShowCard)
                {
                    Card.Visibility = Visibility.Collapsed;
                }

                Card.Opacity = 1;
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

            }
        }

        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                await Task.Delay(100);
                if (clickToArtist) return;
                if (Audio.Url == String.Empty)
                {
                    var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
                    var vkService = StaticService.Container.Resolve<VkService>();

                    navigationService.OpenModal(new TrackNotAvalibleModal(vkService, navigationService, Audio.TrackCode, Audio.OwnerId + "_" + Audio.Id + "_" + Audio.AccessKey), 280, 550);

                    return;
                }
                var player = StaticService.Container.Resolve<PlayerService>();

                await player.PlayTrack(Audio);
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }

        private void Artists_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (Artists == null) return;

                Artists.TextDecorations.Add(TextDecorations.Underline);
                this.Cursor = Cursors.Hand;
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

            }

        }

        private void Artists_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (Artists == null) return;
                foreach (var dec in TextDecorations.Underline)
                {
                    Artists.TextDecorations.Remove(dec);
                }
                this.Cursor = Cursors.Arrow;
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        bool clickToArtist = false;
        private async void Artists_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                clickToArtist = true;
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                if (Audio.MainArtists == null)
                {
                    await navigationService.OpenSearchSection(Audio.Artist);
                }
                else
                {
                    await navigationService.OpenArtistSection(Audio.MainArtists[0].Id);
                }

                clickToArtist = false;
            }
            catch (Exception ex)
            {
                clickToArtist = false;

                logger.Error(ex, ex.Message);
            }
        }
    }
}
