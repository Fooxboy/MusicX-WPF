﻿using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
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
            var notificationService = StaticService.Container.Resolve<Services.NotificationsService>();

            notificationService.Show("Невозможно воспроизвести подкаст", $"Music X пока что не умеет воспроизводить подкасты.");
        }
    }
}