using MusicX.Core.Models;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для ListTracks.xaml
    /// </summary>
    public partial class ListTracks : UserControl
    {
        private readonly Logger logger;

        public ListTracks()
        {
            InitializeComponent();
            logger = StaticService.Container.GetRequiredService<Logger>();
            this.Unloaded += ListTracks_Unloaded;

        }

        private void ListTracks_Unloaded(object sender, RoutedEventArgs e)
        {
            this.StackPanelTracks.Children.Clear();
        }

        public static readonly DependencyProperty TracksProperty =
         DependencyProperty.Register("Tracks", typeof(List<Audio>), typeof(ListTracks), new PropertyMetadata(new List<Audio>()));



        public static readonly DependencyProperty ShowChartProperty =
          DependencyProperty.Register("ShowChart", typeof(bool), typeof(ListPlaylists), new PropertyMetadata(false));

        public bool ShowChart
        {
            get { return (bool)GetValue(ShowChartProperty); }
            set
            {
                SetValue(ShowChartProperty, value);
            }
        }

        public List<Audio> Tracks
        {
            get { return (List<Audio>)GetValue(TracksProperty); }
            set
            {
                SetValue(TracksProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var audios in Tracks.Take(15).Chunk(3))
                {
                    var panel = new StackPanel() {Margin = new Thickness(0, 0, 10, 0)};

                    foreach (var audio in audios)
                    {
                        var chart = 0;
                        if (ShowChart) chart = Tracks.IndexOf(audio) + 1;
                        
                        panel.Children.Add(new TrackControl()
                        {
                            ShowCard = true, 
                            ChartPosition = chart, 
                            Margin = new Thickness(0, 0, 0, 10), 
                            Width = 300, 
                            Height = 60, 
                            Audio = audio
                        });
                    }

                    StackPanelTracks.Children.Add(panel);
                }
            }catch (Exception ex)
            {
                logger.Error("Fail load list tracks");
                logger.Error(ex, ex.Message);
            }
            
        }
    }
}
