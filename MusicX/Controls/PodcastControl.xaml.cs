using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Services;
using Wpf.Ui.Contracts;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для PodcastControl.xaml
    /// </summary>
    public partial class PodcastControl : UserControl
    {
        public PodcastControl()
        {
            InitializeComponent();
            this.Loaded += PodcastControl_Loaded;
        }

        public PodcastEpisode Podcast { get; set; }

        private void PodcastControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Podcast == null)
            {
                this.TitlePodcast.Text = "Вам повезет!";
                this.Author.Text = "Послушать случайный эпизод";
                Placeholder.Fill = Brushes.LightBlue;
                return;

            }
            this.PodcastCover.ImageSource = new BitmapImage(new Uri(Podcast.PodcastInfo.Cover.Sizes[3].Url));
            this.TitlePodcast.Text = Podcast.Title;
            this.Author.Text = Podcast.Artist;
            this.Time.Text = new DateTime(TimeSpan.FromSeconds(Podcast.Duration).Ticks).ToString("HH:mm:ss");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

            snackbarService.Show("Невозможно воспроизвести подкаст",
                "Music X пока что не умеет воспроизводить подкасты.");
        }
    }
}
