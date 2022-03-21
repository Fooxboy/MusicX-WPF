using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
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
    /// Логика взаимодействия для ListTracks.xaml
    /// </summary>
    public partial class ListTracks : UserControl
    {
        private readonly Logger logger;

        public ListTracks()
        {
            InitializeComponent();
            logger = StaticService.Container.Resolve<Logger>();
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
                int currentTrack = 0;
                StackPanel currentStackPanel = null;

                int indexTrack = 0;

                foreach (var track in Tracks)
                {
                    var chart = 0;
                    if (ShowChart) chart = Tracks.IndexOf(track) + 1;

                    indexTrack++;

                    if (currentTrack == 0)
                    {
                        currentStackPanel = new StackPanel() { Margin = new Thickness(0, 0, 10, 0) };
                    }

                    var audio = new TrackControl() { ShowCard = true, ChartPosition = chart, Margin = new Thickness(0, 0, 0, 10), Width = 300, Height = 60, Audio = track };

                    currentStackPanel.Children.Add(audio);
                    currentTrack++;


                    if (currentTrack == 3)
                    {
                        StackPanelTracks.Children.Add(currentStackPanel);

                        currentStackPanel = new StackPanel();
                        currentTrack = 0;

                    }

                    if (indexTrack == 15) break;
                }
            }catch (Exception ex)
            {
                logger.Error("Fail load list tracks");
                logger.Error(ex, ex.Message);
            }
            
        }
    }
}
