using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            logger = StaticService.Container.Resolve<Logger>();

            this.Unloaded += PlaylistControl_Unloaded;
        }

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.CoverImage.ImageSource = null;
            this.CoverImage = null;
            this.CoverImageCompact.ImageSource = null;
            this.CoverImageCompact = null;
            this.Title = null;
            this.TitleCompact = null;
            this.Artist = null;
            this.ArtistCompact = null;
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

        public bool ShowFull { get; set; } = false;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (ShowFull)
                {
                    Card.Opacity = 0;
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
                        Artist.Text = "";

                    }

                    

                    return;
                }
                else
                {
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
                        ArtistCompact.Text = "";

                    }
                }
            }catch (Exception ex)
            {
                logger.Error("Failed load playlist control");
                logger.Error(ex, ex.Message);
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Card.Visibility = Visibility.Visible;
            Card.Opacity = 0.5;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Card.Visibility = Visibility.Collapsed;
            Card.Opacity = 0;
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            var notificationService = StaticService.Container.Resolve<Services.NavigationService>();

            notificationService.NavigateToPage(new PlaylistView(Playlist));
        }

        private void FullGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var notificationService = StaticService.Container.Resolve<Services.NavigationService>();

            notificationService.NavigateToPage(new PlaylistView(Playlist));
        }
    }
}
