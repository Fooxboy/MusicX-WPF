using MusicX.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        public static readonly DependencyProperty VideoProperty = DependencyProperty.Register(
            nameof(Video), typeof(Video), typeof(VideoControl));

        public Video Video
        {
            get => (Video)GetValue(VideoProperty);
            set => SetValue(VideoProperty, value);
        }
        public VideoControl()
        {
            InitializeComponent();
            this.Loaded += VideoControl_Loaded;
        }

        private void VideoControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoImage.ImageSource = new BitmapImage(new Uri(Video.Image[3].Url));
                Time.Text = TimeSpan.FromSeconds(Video.Duration).ToString("m\\:ss");
                NameVideo.Text = Video.Title;
                AuthorVideo.Text = Video.MainArtists[0].Name;
                ReleaseDate.Text = DateTimeOffset.FromUnixTimeSeconds(Video.ReleaseDate).ToString("yyyy");
                
                if (Video.Genres?.Count > 0)
                {
                    Genre.Text = string.Join(" ,", Video.Genres.Select(b => b.Name));
                }
                else
                {
                    Genre.Visibility = Visibility.Collapsed;
                }
            }catch (Exception ex)
            {
                NameVideo.Text = Video.Title;
                AuthorVideo.Text = "";
            }
           
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Video.Player,
                UseShellExecute = true
            });
        }

        private void AuthorVideo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            AuthorVideo.TextDecorations.Add(TextDecorations.Underline);
        }

        private void AuthorVideo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            AuthorVideo.TextDecorations.Clear();
        }

        private void AuthorVideo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            
            navigationService.OpenSection(Video.MainArtists[0].Id, SectionType.Artist);
        }
    }
}
