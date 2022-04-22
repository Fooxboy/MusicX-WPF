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
    /// Логика взаимодействия для ListPlaylists.xaml
    /// </summary>
    public partial class ListPlaylists : UserControl
    {
        private readonly Logger logger;

        public ListPlaylists()
        {
            InitializeComponent();
            logger = StaticService.Container.Resolve<Logger>();
            this.Unloaded += ListPlaylists_Unloaded;

        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.StackPanelPlaylists.Children.Clear();
            //this.StackPanelPlaylists = null;
        }

        public static readonly DependencyProperty PlaylistsProperty =
          DependencyProperty.Register("Playlists", typeof(List<Playlist>), typeof(ListPlaylists), new PropertyMetadata(new List<Playlist>()));

        public List<Playlist> Playlists
        {
            get { return (List<Playlist>)GetValue(PlaylistsProperty); }
            set
            {
                SetValue(PlaylistsProperty, value);
            }
        }

        public static readonly DependencyProperty ShowFullProperty =
         DependencyProperty.Register("ShowFull", typeof(bool), typeof(ListPlaylists), new PropertyMetadata(false));

        public bool ShowFull
        {
            get { return (bool)GetValue(ShowFullProperty); }
            set
            {
                SetValue(ShowFullProperty, value);
            }
        }

        public bool ShowChart { get; set; } = false;


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                if (ShowFull)
                {
                    StackPanelPlaylists.Orientation = Orientation.Vertical;

                    var content = new List<object>();


                    foreach (var playlist in Playlists)
                    {

                        StackPanelPlaylists.Children.Add(new Rectangle() { Height = 1, Fill = Brushes.White, Margin = new Thickness(5, 5, 5, 15), Opacity = 0.1 });

                    }

                    
                    if (Playlists.Count > 0)
                    {
                        var index = StackPanelPlaylists.Children.Count - 1;
                        StackPanelPlaylists.Children.RemoveAt(index);
                    }
                    

                    return;
                }

                int count = 0;

                int chartPosition = 0;

                foreach (var playlist in Playlists)
                {
                    if (count >= 11) break;


                    if (ShowChart)
                    {
                        chartPosition++;

                        StackPanelPlaylists.Children.Add(new PlaylistControl() { Playlist = playlist, ChartPosition = chartPosition.ToString(), Height = 250, Width = 200, Margin = new Thickness(0, 0, 10, 0) });
                    }
                    else
                    {
                        StackPanelPlaylists.Children.Add(new PlaylistControl() { Playlist = playlist, Height = 250, Width = 200, Margin = new Thickness(0, 0, 10, 0) });

                    }

                    
                    count++;
                }
            }catch (Exception ex)
            {
                logger.Error("Failed load list playlists control");
                logger.Error(ex, ex.Message);
            }
        }
    }
}
