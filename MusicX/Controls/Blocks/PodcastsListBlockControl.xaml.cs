
using MusicX.Core.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для PodcastsListBlockControl.xaml
    /// </summary>
    public partial class PodcastsListBlockControl : UserControl
    {
        public PodcastsListBlockControl()
        {
            InitializeComponent();
            this.Loaded += PodcastsListBlockControl_Loaded;
        }

        public bool IsSlider { get; set; }

        public List<PodcastEpisode> Podcasts { get; set; }

        private void PodcastsListBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(IsSlider) ListAllPodcasts.Orientation = Orientation.Horizontal;
            foreach(var podcast in Podcasts)
            {
                if(IsSlider)
                {
                    this.ListAllPodcasts.Children.Add(new PodcastControl() { Height = 120, Width = 400, Margin = new Thickness(0, 0, 10, 0), Podcast = podcast });

                }else
                {
                    this.ListAllPodcasts.Children.Add(new PodcastControl() { Height = 120, Margin = new Thickness(0, 10, 15, 0), Podcast = podcast });

                }
            }
        }
    }
}
